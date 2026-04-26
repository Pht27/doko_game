using Doko.Domain.Lobby;
using Doko.Domain.Players;

namespace Doko.Application.Lobbies.Handlers;

public record RemoveOpaCommand(LobbyId LobbyId, int SeatIndex);

public record RemoveOpaResult(PlayerSeat Seat);

public interface IRemoveOpaHandler
{
    Task<LobbyActionResult<RemoveOpaResult>> ExecuteAsync(
        RemoveOpaCommand command,
        CancellationToken ct = default
    );
}

public sealed class RemoveOpaHandler(ILobbyRepository repository) : IRemoveOpaHandler
{
    public async Task<LobbyActionResult<RemoveOpaResult>> ExecuteAsync(
        RemoveOpaCommand command,
        CancellationToken ct = default
    )
    {
        var lobby = await repository.GetAsync(command.LobbyId, ct);
        if (lobby is null)
            return new LobbyActionResult<RemoveOpaResult>.Failure(LobbyError.LobbyNotFound);

        if (!lobby.TryRemoveOpa(command.SeatIndex, out var seat))
            return new LobbyActionResult<RemoveOpaResult>.Failure(LobbyError.PlayerNotInLobby);

        lobby.RemoveLobbyStartVote(seat);
        lobby.RemoveNewGameVote(seat);

        await repository.SaveAsync(lobby, ct);
        return new LobbyActionResult<RemoveOpaResult>.Ok(new RemoveOpaResult(seat));
    }
}
