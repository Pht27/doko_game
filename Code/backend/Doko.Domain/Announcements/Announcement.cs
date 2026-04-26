using Doko.Domain.Players;

namespace Doko.Domain.Announcements;

public record Announcement(
    PlayerSeat Player,
    AnnouncementType Type,
    int TrickNumber,
    int CardIndexInTrick
)
{
    /// <summary>True when the announcement was forced by the Pflichtansage rule.</summary>
    public bool IsMandatory { get; init; } = false;

    /// <summary>
    /// False for Re players without ♣ Queen in Kontrasolo — their announcements only change their
    /// own button state and have no effect on scoring or Absagen evaluation.
    /// True in all other game modes and for Re players who hold a ♣ Queen.
    /// </summary>
    public bool IsEffective { get; init; } = true;
}
