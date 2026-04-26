using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Parties;

/// <summary>
/// Stille Hochzeit party resolution: the Hochzeit player is Re (solo); all others are Kontra.
/// Parties are fixed from the start — fully resolved immediately.
/// Genscherdamen may fire during the game, but their party swap is suppressed by GameState
/// so this resolver stays authoritative throughout.
/// </summary>
public sealed class StilleHochzeitPartyResolver(PlayerSeat hochzeitPlayer) : IPartyResolver
{
    public Party? ResolveParty(PlayerSeat player, GameState state) =>
        player == hochzeitPlayer ? Party.Re : Party.Kontra;

    public bool IsFullyResolved(GameState state) => true;

    public int? AnnouncementBaseDeadline(GameState state) => 5;
}
