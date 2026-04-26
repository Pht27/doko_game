using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>
/// If a ♠ Queen wins a trick containing a ♠ King that belonged to the opposing party,
/// that party gets +1 point.
/// Uses <paramref name="effectiveTrickWinner"/> so Meuterei correctly prevents this award
/// when the ♠Q does not actually win the trick.
/// </summary>
public sealed class KlabautermannExtrapunkt : IExtrapunkt
{
    private static readonly CardType PikDame = new(Suit.Pik, Rank.Dame);
    private static readonly CardType PikKoenig = new(Suit.Pik, Rank.Koenig);

    public ExtrapunktType Type => ExtrapunktType.Klabautermann;
    public bool UsesFinalPartyState => true;

    public IReadOnlyList<ExtrapunktAward> Evaluate(
        Trick completedTrick,
        GameState state,
        PlayerSeat effectiveTrickWinner
    )
    {
        var winnerParty = state.PartyResolver.ResolveParty(effectiveTrickWinner, state);

        bool winnerPlayedPikDame = completedTrick.Cards.Any(tc =>
            tc.Card.Type == PikDame && tc.Player == effectiveTrickWinner
        );
        if (!winnerPlayedPikDame)
            return [];

        var awards = new List<ExtrapunktAward>();
        foreach (var tc in completedTrick.Cards)
        {
            if (tc.Card.Type != PikKoenig)
                continue;
            var kingParty = state.PartyResolver.ResolveParty(tc.Player, state);
            if (kingParty is not null && kingParty != winnerParty)
                awards.Add(new ExtrapunktAward(Type, effectiveTrickWinner, 1));
        }
        return awards;
    }
}
