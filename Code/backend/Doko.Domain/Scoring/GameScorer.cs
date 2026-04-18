using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.Extrapunkte;
using Doko.Domain.Parties;
using Doko.Domain.Reservations;

namespace Doko.Domain.Scoring;

/// <summary>
/// Calculates <see cref="GameResult"/> from a <see cref="CompletedGame"/>.
/// Game value components: Gewonnen, Gegen die Alten, threshold bonuses (Keine90/60/30, Schwarz),
/// one point per announcement, and net Extrapunkte offset (Re vs. Kontra).
/// Feigheit is checked and reflected in the result.
/// In Soli the solo player's score is tripled by the recording layer; GameValue is the per-opponent value.
/// </summary>
public sealed class GameScorer : IGameScorer
{
    public GameResult Score(CompletedGame game)
    {
        var state = game.FinalState;

        // ── 1. Sum Augen and tricks per party ────────────────────────────────────
        int reAugen = 0;
        int kontraAugen = 0;
        int reStiche = 0;
        int kontraStiche = 0;

        foreach (var trickResult in game.Tricks)
        {
            int trickAugen = ComputeEffectiveAugen(trickResult.Trick, state.CardPointTransfers);
            var winnerParty = state.PartyResolver.ResolveParty(trickResult.Winner, state);
            if (winnerParty == Party.Re)
            {
                reAugen += trickAugen;
                reStiche++;
            }
            else
            {
                kontraAugen += trickAugen;
                kontraStiche++;
            }
        }

        // ── 2. Determine winner ───────────────────────────────────────────────────
        // Re needs 121+; Kontra wins if Re does not reach 121.
        Party winner = reAugen >= 121 ? Party.Re : Party.Kontra;
        int loserAugen = winner == Party.Re ? kontraAugen : reAugen;

        // ── 3. Collect all Extrapunkte awards ─────────────────────────────────────
        var allAwards = game
            .Tricks.SelectMany(t => t.Awards)
            .Where(a => a.Delta > 0) // Delta=0 awards are trick-winner overrides, not score bonuses
            .ToList();

        int reExtra = allAwards
            .Where(a => state.PartyResolver.ResolveParty(a.BenefittingPlayer, state) == Party.Re)
            .Sum(a => a.Delta);
        int kontraExtra = allAwards
            .Where(a =>
                state.PartyResolver.ResolveParty(a.BenefittingPlayer, state) == Party.Kontra
            )
            .Sum(a => a.Delta);

        // ── 4. Build base game value ───────────────────────────────────────────────
        int gameValue = 0;
        var components = new List<GameValueComponent>();

        // Gewonnen
        gameValue++;
        components.Add(new("Gewonnen", 1));

        // Gegen die Alten (Kontra party wins)
        if (winner == Party.Kontra)
        {
            gameValue++;
            components.Add(new("Gegen die Alten", 1));
        }

        // Threshold bonuses (loser didn't reach the threshold)
        if (loserAugen < 90)
        {
            gameValue++;
            components.Add(new("Keine 90", 1));
        }
        if (loserAugen < 60)
        {
            gameValue++;
            components.Add(new("Keine 60", 1));
        }
        if (loserAugen < 30)
        {
            gameValue++;
            components.Add(new("Keine 30", 1));
        }
        if (loserAugen == 0)
        {
            gameValue++;
            components.Add(new("Schwarz", 1));
        }

        // One point per announcement (by any party)
        var announcementRecords = new List<AnnouncementRecord>();
        foreach (var announcement in state.Announcements)
        {
            var party = state.PartyResolver.ResolveParty(announcement.Player, state);
            if (party is null)
                continue;
            gameValue++;
            announcementRecords.Add(new(party.Value, announcement.Type));
        }

        // ── 5. SoloFactor ─────────────────────────────────────────────────────────
        int soloFactor = state.ActiveReservation?.IsSolo == true ? 3 : 1;

        // ── 6. Feigheit ────────────────────────────────────────────────────────────
        var provisionalResult = new GameResult(
            winner,
            reAugen,
            kontraAugen,
            reStiche,
            kontraStiche,
            gameValue,
            allAwards,
            Feigheit: false,
            components,
            soloFactor,
            TotalScore: 0, // placeholder; recomputed below
            announcementRecords
        );

        bool feigheit = AnnouncementRules.ViolatesFeigheit(provisionalResult, state);
        if (feigheit)
        {
            // Feigheit: winning party actually loses
            winner = winner == Party.Re ? Party.Kontra : Party.Re;
            loserAugen = winner == Party.Re ? kontraAugen : reAugen;

            // Recalculate extra penalty: each announcement beyond 2 that was missing adds 1
            int extraPenalty = ComputeFeigheitPenalty(provisionalResult, state);
            gameValue = extraPenalty; // only the penalty counts in a Feigheit loss

            components = [new("Feigheit", gameValue)];
        }

        // ── 7. TotalScore ─────────────────────────────────────────────────────────
        // Gesamtergebnis = (Spielwert + winnerExtra - loserExtra) × SoloFaktor
        // Extrapunkte are multiplied by the solo factor (solo player earns/pays 3×)
        int finalWinnerExtra = winner == Party.Re ? reExtra : kontraExtra;
        int finalLoserExtra = winner == Party.Re ? kontraExtra : reExtra;
        int totalScore = (gameValue + finalWinnerExtra - finalLoserExtra) * soloFactor;

        return new GameResult(
            winner,
            reAugen,
            kontraAugen,
            reStiche,
            kontraStiche,
            gameValue,
            allAwards,
            feigheit,
            components,
            soloFactor,
            totalScore,
            announcementRecords
        );
    }

    private static int ComputeEffectiveAugen(
        Tricks.Trick trick,
        IReadOnlyList<Sonderkarten.TransferCardPointsModification> transfers
    )
    {
        return trick.Cards.Sum(tc => EffectiveCardPoints(tc.Card.Type, transfers));
    }

    private static int EffectiveCardPoints(
        CardType type,
        IReadOnlyList<Sonderkarten.TransferCardPointsModification> transfers
    )
    {
        int points = CardPoints.Of(type.Rank);
        foreach (var t in transfers)
        {
            if (type == t.From)
                return 0;
            if (type == t.To)
                points += CardPoints.Of(t.From.Rank);
        }
        return points;
    }

    private static int ComputeFeigheitPenalty(
        GameResult provisionalResult,
        GameFlow.GameState state
    )
    {
        // Count how many announcements were missing beyond the threshold
        var provisionalWinner = provisionalResult.Winner;
        int loserAugen =
            provisionalWinner == Party.Re
                ? provisionalResult.KontraAugen
                : provisionalResult.ReAugen;

        var winnerAnnounced = state
            .Announcements.Where(a =>
                state.PartyResolver.ResolveParty(a.Player, state) == provisionalWinner
            )
            .Select(a => a.Type)
            .ToHashSet();

        int missing = 0;
        if (loserAugen < 120 && !winnerAnnounced.Contains(AnnouncementType.Win))
            missing++;
        if (loserAugen < 90 && !winnerAnnounced.Contains(AnnouncementType.Keine90))
            missing++;
        if (loserAugen < 60 && !winnerAnnounced.Contains(AnnouncementType.Keine60))
            missing++;
        if (loserAugen < 30 && !winnerAnnounced.Contains(AnnouncementType.Keine30))
            missing++;

        bool loserWonNoTricks = !state.CompletedTricks.Any(t =>
            state.PartyResolver.ResolveParty(
                t.Winner(state.TrumpEvaluator, state.Rules.DulleRule),
                state
            ) != provisionalWinner
        );
        if (loserWonNoTricks && !winnerAnnounced.Contains(AnnouncementType.Schwarz))
            missing++;

        // Each missing beyond 2 adds an extra penalty point
        return Math.Max(0, missing - 2);
    }
}
