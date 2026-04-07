using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Events;

public sealed record ArmutCardsExchangedEvent(
    GameId GameId,
    PlayerId RichPlayer,
    int CardCount,
    bool IncludedTrump
) : IDomainEvent;
