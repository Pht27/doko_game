using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Parties;

/// <summary>Solo party resolution: the solo player is Re; all others are Kontra.</summary>
public sealed class SoloPartyResolver : IPartyResolver
{
    private readonly PlayerId _soloPlayer;

    public SoloPartyResolver(PlayerId soloPlayer) => _soloPlayer = soloPlayer;

    public Party? ResolveParty(PlayerId player, GameState state) =>
        player == _soloPlayer ? Party.Re : Party.Kontra;

    public bool IsFullyResolved(GameState state) => true;
}
