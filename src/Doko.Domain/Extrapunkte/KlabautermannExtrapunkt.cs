using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>
/// If a ♠ Queen wins a trick containing a ♠ King that belonged to the opposing party,
/// that party gets +1 point.
/// </summary>
public sealed class KlabautermannExtrapunkt : IExtrapunkt
{
    private static readonly CardType PikDame   = new(Suit.Pik, Rank.Dame);
    private static readonly CardType PikKoenig = new(Suit.Pik, Rank.Koenig);

    public ExtrapunktType Type => ExtrapunktType.Klabautermann;

    public IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state)
    {
        var winner = completedTrick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
        var winnerParty = state.PartyResolver.ResolveParty(winner, state);

        bool winnerPlayedPikDame = completedTrick.Cards
            .Any(tc => tc.Card.Type == PikDame && tc.Player == winner);
        if (!winnerPlayedPikDame) return [];

        var awards = new List<ExtrapunktAward>();
        foreach (var tc in completedTrick.Cards)
        {
            if (tc.Card.Type != PikKoenig) continue;
            var kingParty = state.PartyResolver.ResolveParty(tc.Player, state);
            if (kingParty is not null && kingParty != winnerParty)
                awards.Add(new ExtrapunktAward(Type, winner, 1));
        }
        return awards;
    }
}
