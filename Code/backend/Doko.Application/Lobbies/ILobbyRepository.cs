using Doko.Domain.Lobby;

namespace Doko.Application.Lobbies;

public interface ILobbyRepository
{
    Task<LobbyState?> GetAsync(LobbyId id, CancellationToken ct = default);
    Task<IReadOnlyList<LobbyState>> GetAllAsync(CancellationToken ct = default);
    Task SaveAsync(LobbyState lobby, CancellationToken ct = default);
    Task DeleteAsync(LobbyId id, CancellationToken ct = default);
}
