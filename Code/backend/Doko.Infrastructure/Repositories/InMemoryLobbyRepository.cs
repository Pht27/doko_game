using System.Collections.Concurrent;
using Doko.Application.Lobbies;
using Doko.Domain.Lobby;

namespace Doko.Infrastructure.Repositories;

public sealed class InMemoryLobbyRepository : ILobbyRepository
{
    private readonly ConcurrentDictionary<LobbyId, LobbyState> _store = new();

    public Task<LobbyState?> GetAsync(LobbyId id, CancellationToken ct = default) =>
        Task.FromResult(_store.GetValueOrDefault(id));

    public Task SaveAsync(LobbyState lobby, CancellationToken ct = default)
    {
        _store[lobby.Id] = lobby;
        return Task.CompletedTask;
    }
}
