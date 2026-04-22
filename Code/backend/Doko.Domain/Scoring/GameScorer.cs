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
            int trickAugen = ComputeEffectiveAugen(trickResult.Trick);
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
        // If a party made Absagen (Keine90+) that were not fulfilled, they lose automatically.
        // Otherwise Re needs 121+; Kontra wins if Re does not reach 121.
        Party winner = DetermineWinner(state, reAugen, kontraAugen);
        int loserAugen = winner == Party.Re ? kontraAugen : reAugen;

        // ── 3. Collect all Extrapunkte awards ─────────────────────────────────────
        // Extrapunkte that check party membership (UsesFinalPartyState=true) are re-evaluated
        // using the final state so Genscher team changes are reflected correctly.
        var activeExtrapunkte = ExtrapunktRegistry.GetActive(state.Rules, state.ActiveReservation);
        var finalStateExtrapunkte = activeExtrapunkte.Where(e => e.UsesFinalPartyState).ToList();
        var finalStateTypeSet = finalStateExtrapunkte.Select(e => e.Type).ToHashSet();

        var allAwards = game
            .Tricks.SelectMany(t => t.Awards)
            .Where(a => a.Delta > 0 && !finalStateTypeSet.Contains(a.Type))
            .Concat(
                game.Tricks.SelectMany(tr =>
                    finalStateExtrapunkte
                        .SelectMany(e => e.Evaluate(tr.Trick, state))
                        .Where(a => a.Delta > 0)
                )
            )
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

        // One point per effective announcement (by any party).
        // Button-only announcements (IsEffective=false) are excluded — they have no scoring effect.
        var announcementRecords = new List<AnnouncementRecord>();
        foreach (var announcement in state.Announcements.Where(a => a.IsEffective))
        {
            var party = state.PartyResolver.ResolveParty(announcement.Player, state);
            if (party is null)
                continue;
            gameValue++;
            announcementRecords.Add(new(party.Value, announcement.Type));
        }

        // ── 5. SoloFactor ─────────────────────────────────────────────────────────
        int soloFactor =
            state.ActiveReservation?.IsSolo == true
            || state.SilentMode is not null
            || state.HochzeitBecameForcedSolo
                ? 3
                : 1;

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

    private static Party DetermineWinner(GameFlow.GameState state, int reAugen, int kontraAugen)
    {
        bool reFailedAbsagen = HasUnfulfilledAbsagen(Party.Re, kontraAugen, state);
        bool kontraFailedAbsagen = HasUnfulfilledAbsagen(Party.Kontra, reAugen, state);

        if (reFailedAbsagen && !kontraFailedAbsagen)
            return Party.Kontra;
        if (kontraFailedAbsagen && !reFailedAbsagen)
            return Party.Re;

        return reAugen >= 121 ? Party.Re : Party.Kontra;
    }

    private static bool HasUnfulfilledAbsagen(
        Party party,
        int opponentAugen,
        GameFlow.GameState state
    )
    {
        var absagen = state
            .Announcements.Where(a =>
                a.IsEffective && state.PartyResolver.ResolveParty(a.Player, state) == party
            )
            .Select(a => a.Type)
            .ToHashSet();

        if (absagen.Contains(AnnouncementType.Keine90) && opponentAugen >= 90)
            return true;
        if (absagen.Contains(AnnouncementType.Keine60) && opponentAugen >= 60)
            return true;
        if (absagen.Contains(AnnouncementType.Keine30) && opponentAugen >= 30)
            return true;
        if (absagen.Contains(AnnouncementType.Schwarz) && opponentAugen != 0)
            return true;

        return false;
    }

    private static int ComputeEffectiveAugen(Tricks.Trick trick)
    {
        return trick.Cards.Sum(tc => CardPoints.Of(tc.Card.Type.Rank));
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
                a.IsEffective
                && state.PartyResolver.ResolveParty(a.Player, state) == provisionalWinner
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
