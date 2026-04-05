using Doko.Application.Abstractions;
using Doko.Domain.GameFlow;

namespace Doko.Application.Tests.Fakes;

public sealed class RecordingGameEventPublisher : IGameEventPublisher
{
    private readonly List<IDomainEvent> _published = [];

    public IReadOnlyList<IDomainEvent> Published => _published;

    public Task PublishAsync(GameId gameId, IReadOnlyList<IDomainEvent> events, CancellationToken ct = default)
    {
        _published.AddRange(events);
        return Task.CompletedTask;
    }
}
