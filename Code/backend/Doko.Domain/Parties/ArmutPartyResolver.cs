using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Parties;

/// <summary>Armut party resolution: poor player + rich player form Re; the other two are Kontra.</summary>
public sealed class ArmutPartyResolver : IPartyResolver
{
    private readonly PlayerSeat _poorPlayer;
    private readonly PlayerSeat _richPlayer;

    public ArmutPartyResolver(PlayerSeat poorPlayer, PlayerSeat richPlayer)
    {
        _poorPlayer = poorPlayer;
        _richPlayer = richPlayer;
    }

    public Party? ResolveParty(PlayerSeat player, GameState state) =>
        player == _poorPlayer || player == _richPlayer ? Party.Re : Party.Kontra;

    public bool IsFullyResolved(GameState state) => true;

    public int? AnnouncementBaseDeadline(GameState state) => 5;
}
