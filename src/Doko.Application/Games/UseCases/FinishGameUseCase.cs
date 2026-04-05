using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.UseCases;

/// <summary>
/// Internal use case — called by <see cref="PlayCardUseCase"/> when the last trick completes.
/// Not exposed as a public interface to the Api layer.
/// </summary>
internal sealed class FinishGameUseCase(IGameScorer scorer)
{
    public GameFinishedResult Execute(GameState state)
    {
        var completed = new CompletedGame(state, state.ScoredTricks);
        var result = scorer.Score(completed);

        state.Apply(new AdvancePhaseModification(GamePhase.Finished));

        return new GameFinishedResult(result);
    }
}
