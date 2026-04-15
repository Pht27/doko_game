using Doko.Domain.GameFlow;

namespace Doko.Application.Abstractions;

public interface IGameRepository
{
    Task<GameState?> GetAsync(GameId id, CancellationToken ct = default);
    Task SaveAsync(GameState state, CancellationToken ct = default);
}
