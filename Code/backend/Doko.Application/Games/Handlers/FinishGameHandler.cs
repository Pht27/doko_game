using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
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
        return new GameFinishedResult(result, netPoints);
    }
}
