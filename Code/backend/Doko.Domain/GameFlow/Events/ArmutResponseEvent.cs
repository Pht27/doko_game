using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Events;

public sealed record ArmutResponseEvent(GameId GameId, PlayerSeat Player, bool Accepted)
    : IDomainEvent;
