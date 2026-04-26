using System.Collections.Concurrent;
using Doko.Application.Lobbies;
using Doko.Domain.GameFlow;
using Doko.Domain.Lobby;

namespace Doko.Infrastructure.Repositories;

public sealed class InMemoryLobbyRepository : ILobbyRepository
{
    private readonly ConcurrentDictionary<LobbyId, LobbyState> _store = new();

    public Task<LobbyState?> GetAsync(LobbyId id, CancellationToken ct = default) =>
        Task.FromResult(_store.GetValueOrDefault(id));

    public Task<LobbyState?> GetByGameIdAsync(GameId gameId, CancellationToken ct = default) =>
        Task.FromResult(_store.Values.FirstOrDefault(l => l.ActiveGameId == gameId));

    public Task<IReadOnlyList<LobbyState>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<LobbyState> result = _store.Values.ToList();
        return Task.FromResult(result);
    }

    public Task SaveAsync(LobbyState lobby, CancellationToken ct = default)
    {
        _store[lobby.Id] = lobby;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(LobbyId id, CancellationToken ct = default)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
