using System.Collections.Concurrent;
using Doko.Application.Abstractions;
using Doko.Domain.GameFlow;

namespace Doko.Infrastructure.Repositories;

public sealed class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<GameId, GameState> _store = new();

    public Task<GameState?> GetAsync(GameId id, CancellationToken ct = default)
        => Task.FromResult(_store.GetValueOrDefault(id));

    public Task SaveAsync(GameState state, CancellationToken ct = default)
    {
        _store[state.Id] = state;
        return Task.CompletedTask;
    }
}
