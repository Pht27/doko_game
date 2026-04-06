using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Parties;

/// <summary>
/// Party resolver after Genscherdamen (or Gegengenscherdamen) activates.
/// The Genscher and their chosen partner form Re; the other two form Kontra.
/// This applies regardless of whether the teams actually changed — the Genscher's
/// team is always Re after the call.
/// </summary>
public sealed class GenscherPartyResolver(PlayerId genscher, PlayerId chosenPartner) : IPartyResolver
{
    public Party? ResolveParty(PlayerId player, GameState state)
        => player == genscher || player == chosenPartner ? Party.Re : Party.Kontra;

    public bool IsFullyResolved(GameState state) => true;
}
