using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Parties;

/// <summary>Solo party resolution: the solo player is Re; all others are Kontra.</summary>
public sealed class SoloPartyResolver : IPartyResolver
{
    private readonly PlayerSeat _soloPlayer;

    public SoloPartyResolver(PlayerSeat soloPlayer) => _soloPlayer = soloPlayer;

    public Party? ResolveParty(PlayerSeat player, GameState state) =>
        player == _soloPlayer ? Party.Re : Party.Kontra;

    public bool IsFullyResolved(GameState state) => true;

    public int? AnnouncementBaseDeadline(GameState state) => 5;
}
