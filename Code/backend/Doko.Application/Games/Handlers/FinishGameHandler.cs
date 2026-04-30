using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Modifications;
using Doko.Domain.Reservations;
using Doko.Domain.Scoring;

namespace Doko.Application.Games.Handlers;

public interface IFinishGameHandler
{
    (GameFinishedResult result, GameState nextState) Execute(PlayingState state);
}

public sealed class FinishGameHandler(IGameScorer scorer) : IFinishGameHandler
{
    public (GameFinishedResult result, GameState nextState) Execute(PlayingState state)
    {
        // Transition to Scoring first so the scorer receives a ScoringState
        var scoringState = (ScoringState)state.Apply(new AdvancePhaseModification(GamePhase.Scoring));
        var completed = new CompletedGame(scoringState, scoringState.ScoredTricks);
        var result = scorer.Score(completed);

        var finishedState = (FinishedState)scoringState.Apply(
            new AdvancePhaseModification(GamePhase.Finished)
        );

        var (netPoints, partyPerSeat) = NetPointsCalculator.Calculate(result, finishedState);

        // Rauskommer advances only after Normal and Hochzeit games; Soli and Armut replay with same leader.
        bool advanceRauskommer =
            finishedState.SilentMode is null
            && (
                finishedState.ActiveReservation is null
                || finishedState.ActiveReservation.Priority == ReservationPriority.Hochzeit
            );

        string? gameMode =
            finishedState.ActiveReservation?.Priority.ToString()
            ?? finishedState.SilentMode?.Type.ToString();
        return (
            new GameFinishedResult(result, netPoints, partyPerSeat, advanceRauskommer, gameMode),
            finishedState
        );
    }
}
