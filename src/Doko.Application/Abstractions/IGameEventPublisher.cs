using Doko.Domain.GameFlow;

namespace Doko.Application.Abstractions;

public interface IGameEventPublisher
{
    Task PublishAsync(
        GameId gameId,
        IReadOnlyList<IDomainEvent> events,
        CancellationToken ct = default
    );
}
