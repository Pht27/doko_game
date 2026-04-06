using Doko.Domain.Players;

namespace Doko.Domain.Announcements;

public record Announcement(
    PlayerId Player,
    AnnouncementType Type,
    int TrickNumber,
    int CardIndexInTrick
);
