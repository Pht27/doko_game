using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Parties;

/// <summary>Standard party resolution: Re = players originally dealt ♣ Queen.</summary>
public sealed class NormalPartyResolver : IPartyResolver
{
    public static readonly NormalPartyResolver Instance = new();

    private static readonly CardType KreuzDame = new(Suit.Kreuz, Rank.Dame);

    public Party? ResolveParty(PlayerId player, GameState state)
    {
        if (state.InitialHands is null)
            return null;
        return state.InitialHands[player].Cards.Any(c => c.Type == KreuzDame)
            ? Party.Re
            : Party.Kontra;
    }

    public bool IsFullyResolved(GameState state) => state.InitialHands is not null;
}
