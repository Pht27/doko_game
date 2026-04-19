using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Parties;

public interface IPartyResolver
{
    Party? ResolveParty(PlayerSeat player, GameState state);
    bool IsFullyResolved(GameState state);

    /// <summary>
    /// Returns the base deadline (total cards played, exclusive upper bound) for announcements.
    /// Returns null if announcements are not yet allowed (e.g. Hochzeit before Findungsstich).
    /// Each subsequent announcement shifts the effective deadline forward by 4 (handled by the caller).
    /// </summary>
    int? AnnouncementBaseDeadline(GameState state);
}
