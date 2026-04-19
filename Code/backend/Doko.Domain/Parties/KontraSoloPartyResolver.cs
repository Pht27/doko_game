using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Parties;

/// <summary>
/// Kontrasolo party resolution: the Kontrasolo player is Kontra (solo); all others are Re.
/// Parties are fixed from the start — fully resolved immediately.
/// </summary>
public sealed class KontraSoloPartyResolver(PlayerSeat kontraSoloPlayer) : IPartyResolver
{
    public Party? ResolveParty(PlayerSeat player, GameState state) =>
        player == kontraSoloPlayer ? Party.Kontra : Party.Re;

    public bool IsFullyResolved(GameState state) => true;

    public int? AnnouncementBaseDeadline(GameState state) => 5;
}
