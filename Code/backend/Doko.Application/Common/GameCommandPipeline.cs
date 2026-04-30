using Doko.Application.Abstractions;
using Doko.Domain.GameFlow;

namespace Doko.Application.Common;

internal static class GameCommandPipeline
{
    internal static async Task<GameActionResult<TResult>> RunAsync<TResult>(
        IGameRepository repo,
        IGameEventPublisher publisher,
        GameId gameId,
        GamePhase requiredPhase,
        Func<
            GameState,
            (
                GameActionResult<TResult> result,
                IReadOnlyList<IDomainEvent> events,
                GameState nextState
            )
        > execute,
        CancellationToken ct
    )
    {
        var loaded = await repo.LoadOrFailAsync<TResult>(gameId, ct);
        if (loaded.Failure is not null)
            return loaded.Failure;
        var state = loaded.State!;

        if (state.Phase != requiredPhase)
            return GameActionResultExtensions.Fail<TResult>(GameError.InvalidPhase);

        var (result, events, nextState) = execute(state);
        if (result is GameActionResult<TResult>.Failure)
            return result;

        await repo.SaveAsync(nextState, ct);
        await publisher.PublishAsync(nextState.Id, events, ct);
        return result;
    }
}
