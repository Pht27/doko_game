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

        if (state.ActiveReservation?.Priority == ReservationPriority.SchlankerMartin)
            return ScoreSchlankerMartin(game);

        var (reAugen, kontraAugen, reStiche, kontraStiche) = SumTrickResults(game, state);

        Party winner = DetermineWinner(state, reAugen, kontraAugen);
        int loserAugen = winner == Party.Re ? kontraAugen : reAugen;

        var (allAwards, reExtra, kontraExtra) = CollectExtrapunkteAwards(game, state);

        var (gameValue, components, announcementRecords) = BuildBaseValue(
            state,
            winner,
            loserAugen
        );

        int soloFactor = GetSoloFactor(state);

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
            TotalScore: 0,
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

    private static (int reAugen, int kontraAugen, int reStiche, int kontraStiche) SumTrickResults(
        CompletedGame game,
        GameFlow.GameState state
    )
    {
        int reAugen = 0;
        int kontraAugen = 0;
        int reStiche = 0;
        int kontraStiche = 0;

        foreach (var trickResult in game.Tricks)
        {
            int augen = ComputeEffectiveAugen(trickResult.Trick);
            if (state.PartyResolver.ResolveParty(trickResult.Winner, state) == Party.Re)
            {
                reAugen += augen;
                reStiche++;
            }
            else
            {
                kontraAugen += augen;
                kontraStiche++;
            }
        }

        return (reAugen, kontraAugen, reStiche, kontraStiche);
    }

    private static (
        List<ExtrapunktAward> allAwards,
        int reExtra,
        int kontraExtra
    ) CollectExtrapunkteAwards(CompletedGame game, GameFlow.GameState state)
    {
        var activeExtrapunkte = ExtrapunktRegistry.GetActive(state.Rules, state.ActiveReservation);

        // Extrapunkte that check party membership (UsesFinalPartyState=true) are re-evaluated
        // using the final state so Genscher team changes are reflected correctly.
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

        int reExtra = SumExtraForParty(allAwards, Party.Re, state);
        int kontraExtra = SumExtraForParty(allAwards, Party.Kontra, state);
        return (allAwards, reExtra, kontraExtra);
    }

    private static int SumExtraForParty(
        List<ExtrapunktAward> awards,
        Party party,
        GameFlow.GameState state
    ) =>
        awards
            .Where(a => state.PartyResolver.ResolveParty(a.BenefittingPlayer, state) == party)
            .Sum(a => a.Delta);

    private static (
        int gameValue,
        List<GameValueComponent> components,
        List<AnnouncementRecord> announcementRecords
    ) BuildBaseValue(GameFlow.GameState state, Party winner, int loserAugen)
    {
        int gameValue = 0;
        var components = new List<GameValueComponent>();
        var announcementRecords = new List<AnnouncementRecord>();

        gameValue++;
        components.Add(new("Gewonnen", 1));

        if (winner == Party.Kontra)
        {
            gameValue++;
            components.Add(new("Gegen die Alten", 1));
        }

        AddThresholdBonus(ref gameValue, components, loserAugen, threshold: 90, "Keine 90");
        AddThresholdBonus(ref gameValue, components, loserAugen, threshold: 60, "Keine 60");
        AddThresholdBonus(ref gameValue, components, loserAugen, threshold: 30, "Keine 30");

        if (loserAugen == 0)
        {
            gameValue++;
            components.Add(new("Schwarz", 1));
        }

        // Button-only announcements (IsEffective=false) are excluded — they have no scoring effect.
        foreach (var announcement in state.Announcements.Where(a => a.IsEffective))
        {
            var party = state.PartyResolver.ResolveParty(announcement.Player, state);
            if (party is null)
                continue;
            gameValue++;
            announcementRecords.Add(new(party.Value, announcement.Type));
        }

        return (gameValue, components, announcementRecords);
    }

    private static void AddThresholdBonus(
        ref int gameValue,
        List<GameValueComponent> components,
        int loserAugen,
        int threshold,
        string label
    )
    {
        if (loserAugen < threshold)
        {
            gameValue++;
            components.Add(new(label, 1));
        }
    }

    private static int GetSoloFactor(GameFlow.GameState state) =>
        state.ActiveReservation?.IsSolo == true
        || state.SilentMode is not null
        || state.HochzeitBecameForcedSolo
            ? 3
            : 1;

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

    private static GameResult ScoreSchlankerMartin(CompletedGame game)
    {
        var state = game.FinalState;

        // Count tricks and Augen per player
        var tricksPerPlayer = new Dictionary<Players.PlayerSeat, int>();
        int reAugen = 0;
        int kontraAugen = 0;

        foreach (var trickResult in game.Tricks)
        {
            tricksPerPlayer[trickResult.Winner] =
                tricksPerPlayer.GetValueOrDefault(trickResult.Winner) + 1;

            int augen = ComputeEffectiveAugen(trickResult.Trick);
            if (state.PartyResolver.ResolveParty(trickResult.Winner, state) == Party.Re)
                reAugen += augen;
            else
                kontraAugen += augen;
        }

        // Solo player is the one mapped to Re
        var soloSeat = state
            .Players.Single(p => state.PartyResolver.ResolveParty(p.Seat, state) == Party.Re)
            .Seat;

        int soloTricks = tricksPerPlayer.GetValueOrDefault(soloSeat);
        int kontraMinTricks = state
            .Players.Where(p => p.Seat != soloSeat)
            .Min(p => tricksPerPlayer.GetValueOrDefault(p.Seat));

        // Re wins when solo player has fewest or ties for fewest
        var winner = soloTricks <= kontraMinTricks ? Party.Re : Party.Kontra;

        // Game value 0 on exact tie, 1 when there is a strict difference
        int gameValue = soloTricks == kontraMinTricks ? 0 : 1;

        int reStiche = tricksPerPlayer.GetValueOrDefault(soloSeat);
        int kontraStiche = game.Tricks.Count - reStiche;

        var components =
            gameValue == 1 ? (IReadOnlyList<GameValueComponent>)[new("Gewonnen", 1)] : [];

        int totalScore = gameValue * 3; // soloFactor = 3

        return new GameResult(
            winner,
            reAugen,
            kontraAugen,
            reStiche,
            kontraStiche,
            gameValue,
            AllAwards: [],
            Feigheit: false,
            components,
            SoloFactor: 3,
            totalScore,
            AnnouncementRecords: []
        );
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
