using Doko.Domain.Lobby;

namespace Doko.Application.Lobbies;

public interface ILobbyRepository
{
    Task<LobbyState?> GetAsync(LobbyId id, CancellationToken ct = default);
    Task SaveAsync(LobbyState lobby, CancellationToken ct = default);
}
