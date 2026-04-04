using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Parties;

/// <summary>Armut party resolution: poor player + rich player form Re; the other two are Kontra.</summary>
public sealed class ArmutPartyResolver : IPartyResolver
{
    private readonly PlayerId _poorPlayer;
    private readonly PlayerId _richPlayer;

    public ArmutPartyResolver(PlayerId poorPlayer, PlayerId richPlayer)
    {
        _poorPlayer = poorPlayer;
        _richPlayer = richPlayer;
    }

    public Party? ResolveParty(PlayerId player, GameState state)
        => player == _poorPlayer || player == _richPlayer ? Party.Re : Party.Kontra;

    public bool IsFullyResolved(GameState state) => true;
}
