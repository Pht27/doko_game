using Doko.Application.Abstractions;
using Doko.Domain.GameFlow;

namespace Doko.Application.Common;

internal static class GameCommandPipeline
{
    internal static async Task<GameActionResult<TResult>> RunAsync<TResult, TRequiredState>(
        IGameRepository repo,
        IGameEventPublisher publisher,
        GameId gameId,
        Func<
            TRequiredState,
            (
                GameActionResult<TResult> result,
                IReadOnlyList<IDomainEvent> events,
                GameState nextState
            )
        > execute,
        CancellationToken ct
    )
        where TRequiredState : GameState
    {
        var loaded = await repo.LoadOrFailAsync<TResult>(gameId, ct);
        if (loaded.Failure is not null)
            return loaded.Failure;
        var state = loaded.State!;

        if (state is not TRequiredState typedState)
            return GameActionResultExtensions.Fail<TResult>(GameError.InvalidPhase);

        var (result, events, nextState) = execute(typedState);
        if (result is GameActionResult<TResult>.Failure)
            return result;

        await repo.SaveAsync(nextState, ct);
        await publisher.PublishAsync(nextState.Id, events, ct);
        return result;
    }
}
