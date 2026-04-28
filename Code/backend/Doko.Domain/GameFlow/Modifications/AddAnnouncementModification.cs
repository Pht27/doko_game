using Doko.Domain.Announcements;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>Appends an announcement to the game state.</summary>
public sealed record AddAnnouncementModification(Announcement Announcement) : GameStateModification;
