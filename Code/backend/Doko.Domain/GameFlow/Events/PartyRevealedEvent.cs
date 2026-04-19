using Doko.Domain.Parties;
using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Events;

public sealed record PartyRevealedEvent(GameId GameId, PlayerSeat Player, Party Party)
    : IDomainEvent;
