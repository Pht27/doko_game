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

        // ── 1. Sum Augen per party ────────────────────────────────────────────────
        int rePoints = 0;
        int kontraPoints = 0;

        foreach (var trickResult in game.Tricks)
        {
            int trickAugen = ComputeEffectiveAugen(trickResult.Trick, state.CardPointTransfers);
            var winnerParty = state.PartyResolver.ResolveParty(trickResult.Winner, state);
            if (winnerParty == Party.Re)
                rePoints += trickAugen;
            else
                kontraPoints += trickAugen;
        }

        // ── 2. Determine winner ───────────────────────────────────────────────────
        // Re needs 121+; Kontra wins if Re does not reach 121.
        Party winner = rePoints >= 121 ? Party.Re : Party.Kontra;
        int loserPoints = winner == Party.Re ? kontraPoints : rePoints;

        // ── 3. Determine if loser won any tricks ──────────────────────────────────
        bool loserWonNoTricks = !game.Tricks.Any(t =>
            state.PartyResolver.ResolveParty(t.Winner, state) != winner
        );

        // ── 4. Collect all Extrapunkte awards ─────────────────────────────────────
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

        // ── 5. Build base game value ───────────────────────────────────────────────
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
        if (loserPoints < 90)
        {
            gameValue++;
            components.Add(new("Keine 90", 1));
        }
        if (loserPoints < 60)
        {
            gameValue++;
            components.Add(new("Keine 60", 1));
        }
        if (loserPoints < 30)
        {
            gameValue++;
            components.Add(new("Keine 30", 1));
        }
        if (loserWonNoTricks)
        {
            gameValue++;
            components.Add(new("Schwarz", 1));
        }

        // One point per announcement (by any party)
        int announcementsCount = state.Announcements.Count;
        if (announcementsCount > 0)
        {
            gameValue += announcementsCount;
            components.Add(new("Ansagen", announcementsCount));
        }

        // Net Extrapunkte offset
        int winnerExtra = winner == Party.Re ? reExtra : kontraExtra;
        int loserExtra = winner == Party.Re ? kontraExtra : reExtra;
        int netExtra = winnerExtra - loserExtra;
        if (netExtra != 0)
        {
            gameValue += netExtra;
            components.Add(new("Extrapunkte", netExtra));
        }

        // ── 6. Feigheit ────────────────────────────────────────────────────────────
        var provisionalResult = new GameResult(
            winner,
            rePoints,
            kontraPoints,
            gameValue,
            allAwards,
            Feigheit: false,
            components
        );

        bool feigheit = AnnouncementRules.ViolatesFeigheit(provisionalResult, state);
        if (feigheit)
        {
            // Feigheit: winning party actually loses
            winner = winner == Party.Re ? Party.Kontra : Party.Re;
            loserPoints = winner == Party.Re ? kontraPoints : rePoints;

            // Recalculate extra penalty: each announcement beyond 2 that was missing adds 1
            int extraPenalty = ComputeFeigheitPenalty(provisionalResult, state);
            gameValue = 1 + extraPenalty; // Gewonnen + extra

            components = [new("Gewonnen", 1)];
            if (extraPenalty > 0)
                components.Add(new("Feigheit-Strafe", extraPenalty));
        }

        return new GameResult(
            winner,
            rePoints,
            kontraPoints,
            gameValue,
            allAwards,
            feigheit,
            components
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
        int loserPoints =
            provisionalWinner == Party.Re
                ? provisionalResult.KontraPoints
                : provisionalResult.RePoints;

        var winnerAnnounced = state
            .Announcements.Where(a =>
                state.PartyResolver.ResolveParty(a.Player, state) == provisionalWinner
            )
            .Select(a => a.Type)
            .ToHashSet();

        int missing = 0;
        if (loserPoints < 120 && !winnerAnnounced.Contains(AnnouncementType.Win))
            missing++;
        if (loserPoints < 90 && !winnerAnnounced.Contains(AnnouncementType.Keine90))
            missing++;
        if (loserPoints < 60 && !winnerAnnounced.Contains(AnnouncementType.Keine60))
            missing++;
        if (loserPoints < 30 && !winnerAnnounced.Contains(AnnouncementType.Keine30))
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
