using Doko.Application.Common;
using Doko.Domain.GameFlow;

namespace Doko.Application.Abstractions;

public static class GameRepositoryExtensions
{
    public static async Task<(GameState? State, GameActionResult<T>? Failure)> LoadOrFailAsync<T>(
        this IGameRepository repository,
        GameId id,
        CancellationToken ct
    )
    {
        var state = await repository.GetAsync(id, ct);
        return state is null
            ? (null, GameActionResultExtensions.Fail<T>(GameError.GameNotFound))
            : (state, null);
    }
}
