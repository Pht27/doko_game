using Doko.Domain.Players;

namespace Doko.Domain.Announcements;

public record Announcement(
    PlayerId Player,
    AnnouncementType Type,
    int TrickNumber,
    int CardIndexInTrick
)
{
    /// <summary>True when the announcement was forced by the Pflichtansage rule.</summary>
    public bool IsMandatory { get; init; } = false;
}
