using Doko.Domain.Cards;
using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Events;

public sealed record CardPlayedEvent(GameId GameId, PlayerSeat Player, Card Card, int TrickNumber)
    : IDomainEvent;
