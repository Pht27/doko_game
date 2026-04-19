using Doko.Application.Games.Queries;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Application.Abstractions;

public interface IGameQueryService
{
    Task<PlayerGameView?> GetPlayerViewAsync(
        GameId gameId,
        PlayerSeat requestingPlayer,
        CancellationToken ct = default
    );
}
