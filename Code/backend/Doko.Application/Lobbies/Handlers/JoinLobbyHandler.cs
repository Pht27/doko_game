using Doko.Domain.Lobby;
using Doko.Domain.Players;

namespace Doko.Application.Lobbies.Handlers;

public record JoinLobbyResult(PlayerId PlayerId, bool IsNowFull);

public interface IJoinLobbyHandler
{
    Task<LobbyActionResult<JoinLobbyResult>> ExecuteAsync(
        LobbyId lobbyId,
        CancellationToken ct = default
    );
}

public sealed class JoinLobbyHandler(ILobbyRepository repository) : IJoinLobbyHandler
{
    public async Task<LobbyActionResult<JoinLobbyResult>> ExecuteAsync(
        LobbyId lobbyId,
        CancellationToken ct = default
    )
    {
        var lobby = await repository.GetAsync(lobbyId, ct);
        if (lobby is null)
            return new LobbyActionResult<JoinLobbyResult>.Failure(LobbyError.LobbyNotFound);

        if (lobby.IsStarted)
            return new LobbyActionResult<JoinLobbyResult>.Failure(LobbyError.LobbyAlreadyStarted);

        if (!lobby.TryAddPlayer(out var newPlayerId))
            return new LobbyActionResult<JoinLobbyResult>.Failure(LobbyError.LobbyFull);

        await repository.SaveAsync(lobby, ct);
        return new LobbyActionResult<JoinLobbyResult>.Ok(
            new JoinLobbyResult(newPlayerId, lobby.IsFull)
        );
    }
}
