using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>
/// Each ♦ Ace (Fuchs) that ends up with the opposing party gives that party +1 point.
/// ♦ Aces that have been upgraded to Schweinchen are excluded.
/// </summary>
public sealed class FuchsGefangenExtrapunkt : IExtrapunkt
{
    private static readonly CardType KaroAss = new(Suit.Karo, Rank.Ass);

    public ExtrapunktType Type => ExtrapunktType.FuchsGefangen;
    public bool UsesFinalPartyState => true;

    public IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state)
    {
        // ♦A are Schweinchen when that sonderkarte is active — not Füchse
        if (state.ActiveSonderkarten.Contains(SonderkarteType.Schweinchen))
            return [];

        var winner = completedTrick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
        var winnerParty = state.PartyResolver.ResolveParty(winner, state);
        if (winnerParty is null)
            return [];

        var awards = new List<ExtrapunktAward>();
        foreach (var tc in completedTrick.Cards)
        {
            if (tc.Card.Type != KaroAss)
                continue;
            var foxParty = state.PartyResolver.ResolveParty(tc.Player, state);
            if (foxParty is not null && foxParty != winnerParty)
                awards.Add(new ExtrapunktAward(Type, winner, 1));
        }
        return awards;
    }
}
