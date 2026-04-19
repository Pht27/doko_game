using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.Reservations;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.Handlers;

/// <summary>
/// Internal handler — called by <see cref="PlayCardHandler"/> when the last trick completes.
/// Not exposed as a public interface to the Api layer.
/// </summary>
internal sealed class FinishGameHandler(IGameScorer scorer)
{
    public GameFinishedResult Execute(GameState state)
    {
        var completed = new CompletedGame(state, state.ScoredTricks);
        var result = scorer.Score(completed);

        state.Apply(new AdvancePhaseModification(GamePhase.Finished));

        var netPoints = NetPointsCalculator.Calculate(result, state);

        // Rauskommer advances only after Normal and Hochzeit games; Soli and Armut replay with same leader.
        // Forced Hochzeit solo (partner not found) also replays with same leader.
        bool advanceRauskommer =
            state.SilentMode is null
            && !state.HochzeitBecameForcedSolo
            && (
                state.ActiveReservation is null
                || state.ActiveReservation.Priority == ReservationPriority.Hochzeit
            );

        string? gameMode =
            state.ActiveReservation?.Priority.ToString() ?? state.SilentMode?.Type.ToString();
        return new GameFinishedResult(result, netPoints, advanceRauskommer, gameMode);
    }
}
