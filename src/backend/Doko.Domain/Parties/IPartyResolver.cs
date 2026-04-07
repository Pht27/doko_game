using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Parties;

public interface IPartyResolver
{
    Party? ResolveParty(PlayerId player, GameState state);
    bool IsFullyResolved(GameState state);
}
