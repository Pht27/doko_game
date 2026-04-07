using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Events;

public sealed record HealthDeclaredEvent(GameId GameId, PlayerId Player, bool HasVorbehalt)
    : IDomainEvent;
