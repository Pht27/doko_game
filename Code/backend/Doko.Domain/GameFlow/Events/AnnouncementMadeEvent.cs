using Doko.Domain.Announcements;
using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Events;

public sealed record AnnouncementMadeEvent(
    GameId GameId,
    PlayerSeat Player,
    AnnouncementType Type,
    int TrickNumber,
    int CardIndexInTrick
) : IDomainEvent;
