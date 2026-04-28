using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.Reservations;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.Handlers;

public interface IFinishGameHandler
{
    GameFinishedResult Execute(GameState state);
}

public sealed class FinishGameHandler(IGameScorer scorer) : IFinishGameHandler
{
    public GameFinishedResult Execute(GameState state)
    {
        var completed = new CompletedGame(state, state.ScoredTricks);
        var result = scorer.Score(completed);

        state.Apply(new AdvancePhaseModification(GamePhase.Finished));

        var (netPoints, partyPerSeat) = NetPointsCalculator.Calculate(result, state);

        // Rauskommer advances only after Normal and Hochzeit games; Soli and Armut replay with same leader.
        bool advanceRauskommer =
            state.SilentMode is null
            && (
                state.ActiveReservation is null
                || state.ActiveReservation.Priority == ReservationPriority.Hochzeit
            );

        string? gameMode =
            state.ActiveReservation?.Priority.ToString() ?? state.SilentMode?.Type.ToString();
        return new GameFinishedResult(result, netPoints, partyPerSeat, advanceRauskommer, gameMode);
    }
}
