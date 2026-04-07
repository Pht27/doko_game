using Doko.Application.Games.Queries;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Application.Abstractions;

public interface IGameQueryService
{
    Task<PlayerGameView?> GetPlayerViewAsync(
        GameId gameId,
        PlayerId requestingPlayer,
        CancellationToken ct = default
    );
}
