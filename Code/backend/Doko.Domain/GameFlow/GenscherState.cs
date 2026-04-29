using Doko.Domain.Announcements;
using Doko.Domain.Players;

namespace Doko.Domain.GameFlow;

public sealed record GenscherState(
    bool TeamsChanged,
    (PlayerSeat First, PlayerSeat Second)? PreRePlayers,
    IReadOnlyList<Announcement>? SavedAnnouncements
);
