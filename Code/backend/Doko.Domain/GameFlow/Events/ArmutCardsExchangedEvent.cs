using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Events;

public sealed record ArmutCardsExchangedEvent(
    GameId GameId,
    PlayerSeat RichPlayer,
    int CardCount,
    bool IncludedTrump
) : IDomainEvent;
