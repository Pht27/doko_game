using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>
/// If a Fischauge (♦9) is beaten by exactly one Fuchs (♦A) that wins the trick,
/// and the Fischauge belonged to the opposing party, the Fuchs's party gets +1 point.
/// </summary>
public sealed class GansGefangenExtrapunkt : IExtrapunkt
{
    private static readonly CardType KaroNeun = new(Suit.Karo, Rank.Neun);
    private static readonly CardType KaroAss = new(Suit.Karo, Rank.Ass);

    public ExtrapunktType Type => ExtrapunktType.GansGefangen;
    public bool UsesFinalPartyState => true;

    public IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state)
    {
        if (!AnimalHelpers.FischaugeActive(state))
            return [];
        // ♦A are Schweinchen when active — no Füchse to catch Gänse
        if (state.ActiveSonderkarten.Contains(SonderkarteType.Schweinchen))
            return [];

        var fishes = completedTrick.Cards.Where(tc => tc.Card.Type == KaroNeun).ToList();
        var foxes = completedTrick.Cards.Where(tc => tc.Card.Type == KaroAss).ToList();

        if (fishes.Count == 0 || foxes.Count != 1)
            return [];

        var winner = completedTrick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
        var fox = foxes[0];
        if (fox.Player != winner)
            return [];

        var winnerParty = state.PartyResolver.ResolveParty(winner, state);
        var awards = new List<ExtrapunktAward>();
        foreach (var fish in fishes)
        {
            var fishParty = state.PartyResolver.ResolveParty(fish.Player, state);
            if (fishParty is not null && fishParty != winnerParty)
                awards.Add(new ExtrapunktAward(Type, winner, 1));
        }
        return awards;
    }
}
