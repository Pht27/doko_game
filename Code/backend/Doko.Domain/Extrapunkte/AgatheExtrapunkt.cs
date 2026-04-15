using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>
/// If a ♦ Queen (Agathe) wins the last trick AND a ♣ Jack (Karlchen) from the opposing party
/// is in that trick, the Agathe's party gets +1 point.
/// </summary>
public sealed class AgatheExtrapunkt : IExtrapunkt
{
    private static readonly CardType KaroDame = new(Suit.Karo, Rank.Dame);
    private static readonly CardType KreuzBube = new(Suit.Kreuz, Rank.Bube);

    public ExtrapunktType Type => ExtrapunktType.Agathe;

    public IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state)
    {
        if (state.CompletedTricks.Count != 11)
            return [];

        var winner = completedTrick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
        var winnerParty = state.PartyResolver.ResolveParty(winner, state);

        bool winnerPlayedAgathe = completedTrick.Cards.Any(tc =>
            tc.Card.Type == KaroDame && tc.Player == winner
        );
        if (!winnerPlayedAgathe)
            return [];

        var awards = new List<ExtrapunktAward>();
        foreach (var tc in completedTrick.Cards)
        {
            if (tc.Card.Type != KreuzBube)
                continue;
            var karlchenParty = state.PartyResolver.ResolveParty(tc.Player, state);
            if (karlchenParty is not null && karlchenParty != winnerParty)
                awards.Add(new ExtrapunktAward(Type, winner, 1));
        }
        return awards;
    }
}
