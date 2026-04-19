using Doko.Domain.Lobby;
using Doko.Domain.Players;

namespace Doko.Application.Lobbies.Handlers;

public record LeaveLobbyCommand(LobbyId LobbyId, PlayerSeat PlayerSeat);

public record LeaveLobbyResult(bool LobbyDeleted);

public interface ILeaveLobbyHandler
{
    Task<LobbyActionResult<LeaveLobbyResult>> ExecuteAsync(
        LeaveLobbyCommand command,
        CancellationToken ct = default
    );
}

public sealed class LeaveLobbyHandler(ILobbyRepository repository) : ILeaveLobbyHandler
{
    public async Task<LobbyActionResult<LeaveLobbyResult>> ExecuteAsync(
        LeaveLobbyCommand command,
        CancellationToken ct = default
    )
    {
        var lobby = await repository.GetAsync(command.LobbyId, ct);
        if (lobby is null)
            return new LobbyActionResult<LeaveLobbyResult>.Failure(LobbyError.LobbyNotFound);

        if (!lobby.HasPlayer(command.PlayerSeat))
            return new LobbyActionResult<LeaveLobbyResult>.Failure(LobbyError.PlayerNotInLobby);

        var isNowEmpty = lobby.TryRemovePlayer(command.PlayerSeat);

        if (isNowEmpty)
            await repository.DeleteAsync(command.LobbyId, ct);
        else
            await repository.SaveAsync(lobby, ct);

        return new LobbyActionResult<LeaveLobbyResult>.Ok(new LeaveLobbyResult(isNowEmpty));
    }
}
