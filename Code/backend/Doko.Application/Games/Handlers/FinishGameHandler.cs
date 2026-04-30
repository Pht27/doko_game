using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Modifications;
using Doko.Domain.Reservations;
using Doko.Domain.Scoring;

namespace Doko.Application.Games.Handlers;

public interface IFinishGameHandler
{
    (GameFinishedResult result, GameState nextState) Execute(GameState state);
}

public sealed class FinishGameHandler(IGameScorer scorer) : IFinishGameHandler
{
    public (GameFinishedResult result, GameState nextState) Execute(GameState state)
    {
        var completed = new CompletedGame(state, state.ScoredTricks);
        var result = scorer.Score(completed);

        var nextState = state.Apply(new AdvancePhaseModification(GamePhase.Finished));

        var (netPoints, partyPerSeat) = NetPointsCalculator.Calculate(result, nextState);

        // Rauskommer advances only after Normal and Hochzeit games; Soli and Armut replay with same leader.
        bool advanceRauskommer =
            nextState.SilentMode is null
            && (
                nextState.ActiveReservation is null
                || nextState.ActiveReservation.Priority == ReservationPriority.Hochzeit
            );

        string? gameMode =
            nextState.ActiveReservation?.Priority.ToString()
            ?? nextState.SilentMode?.Type.ToString();
        return (
            new GameFinishedResult(result, netPoints, partyPerSeat, advanceRauskommer, gameMode),
            nextState
        );
    }
}
