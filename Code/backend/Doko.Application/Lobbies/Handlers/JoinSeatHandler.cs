using Doko.Domain.Lobby;
using Doko.Domain.Players;

namespace Doko.Application.Lobbies.Handlers;

public record JoinSeatCommand(LobbyId LobbyId, int SeatIndex);

public record JoinSeatResult(PlayerSeat Seat, bool IsNowFull);

public interface IJoinSeatHandler
{
    Task<LobbyActionResult<JoinSeatResult>> ExecuteAsync(
        JoinSeatCommand command,
        CancellationToken ct = default
    );
}

public sealed class JoinSeatHandler(ILobbyRepository repository) : IJoinSeatHandler
{
    public async Task<LobbyActionResult<JoinSeatResult>> ExecuteAsync(
        JoinSeatCommand command,
        CancellationToken ct = default
    )
    {
        var lobby = await repository.GetAsync(command.LobbyId, ct);
        if (lobby is null)
            return new LobbyActionResult<JoinSeatResult>.Failure(LobbyError.LobbyNotFound);

        if (!lobby.TryOccupySeat(command.SeatIndex, out var playerId))
            return new LobbyActionResult<JoinSeatResult>.Failure(LobbyError.SeatOccupied);

        await repository.SaveAsync(lobby, ct);
        return new LobbyActionResult<JoinSeatResult>.Ok(new JoinSeatResult(playerId, lobby.IsFull));
    }
}
