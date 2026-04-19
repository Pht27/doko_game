using Doko.Domain.Lobby;
using Doko.Domain.Players;

namespace Doko.Application.Lobbies.Handlers;

public record SwapSeatCommand(LobbyId LobbyId, PlayerSeat FromSeat, int ToSeatIndex);

public record SwapSeatResult(PlayerSeat NewSeat);

public interface ISwapSeatHandler
{
    Task<LobbyActionResult<SwapSeatResult>> ExecuteAsync(
        SwapSeatCommand command,
        CancellationToken ct = default
    );
}

public sealed class SwapSeatHandler(ILobbyRepository repository) : ISwapSeatHandler
{
    public async Task<LobbyActionResult<SwapSeatResult>> ExecuteAsync(
        SwapSeatCommand command,
        CancellationToken ct = default
    )
    {
        var lobby = await repository.GetAsync(command.LobbyId, ct);
        if (lobby is null)
            return new LobbyActionResult<SwapSeatResult>.Failure(LobbyError.LobbyNotFound);

        if (!lobby.HasPlayer(command.FromSeat))
            return new LobbyActionResult<SwapSeatResult>.Failure(LobbyError.PlayerNotInLobby);

        // Remove old seat then occupy new seat without ever persisting an empty lobby
        lobby.TryRemovePlayer(command.FromSeat);

        if (!lobby.TryOccupySeat(command.ToSeatIndex, out var newSeat))
        {
            // Target seat is taken — restore original seat and abort
            lobby.TryOccupySeat((int)command.FromSeat, out _);
            return new LobbyActionResult<SwapSeatResult>.Failure(LobbyError.SeatOccupied);
        }

        await repository.SaveAsync(lobby, ct);
        return new LobbyActionResult<SwapSeatResult>.Ok(new SwapSeatResult(newSeat));
    }
}
