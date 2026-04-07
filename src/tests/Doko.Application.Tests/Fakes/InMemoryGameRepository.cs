using System.Collections.Concurrent;
using Doko.Application.Abstractions;
using Doko.Domain.GameFlow;

namespace Doko.Application.Tests.Fakes;

public sealed class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, GameState> _store = new();

    public Task<GameState?> GetAsync(GameId id, CancellationToken ct = default) =>
        Task.FromResult(_store.TryGetValue(id.Value, out var state) ? state : null);

    public Task SaveAsync(GameState state, CancellationToken ct = default)
    {
        _store[state.Id.Value] = state;
        return Task.CompletedTask;
    }
}
