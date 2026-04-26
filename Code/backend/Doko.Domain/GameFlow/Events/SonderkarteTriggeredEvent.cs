using Doko.Domain.Players;
using Doko.Domain.Sonderkarten;

namespace Doko.Domain.GameFlow.Events;

public sealed record SonderkarteTriggeredEvent(
    GameId GameId,
    PlayerSeat Player,
    SonderkarteType Type,
    IReadOnlyList<GameStateModification> Modifications
) : IDomainEvent;
