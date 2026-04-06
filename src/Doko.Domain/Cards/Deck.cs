namespace Doko.Domain.Cards;

public static class Deck
{
    private static readonly Suit[] AllSuits = [Suit.Kreuz, Suit.Pik, Suit.Herz, Suit.Karo];

    private static readonly Rank[] AllRanks =
    [
        Rank.Neun,
        Rank.Bube,
        Rank.Dame,
        Rank.Koenig,
        Rank.Zehn,
        Rank.Ass,
    ];

    private static readonly Rank[] RanksNoNines =
    [
        Rank.Bube,
        Rank.Dame,
        Rank.Koenig,
        Rank.Zehn,
        Rank.Ass,
    ];

    /// <summary>Returns the standard 48-card double deck (two copies of each of the 24 card types).</summary>
    public static IReadOnlyList<Card> Standard48() => BuildDeck(AllRanks);

    /// <summary>Returns the 40-card double deck with no Nines.</summary>
    public static IReadOnlyList<Card> Standard40() => BuildDeck(RanksNoNines);

    private static IReadOnlyList<Card> BuildDeck(Rank[] ranks)
    {
        var cards = new List<Card>(ranks.Length * AllSuits.Length * 2);
        byte id = 0;
        foreach (var suit in AllSuits)
        foreach (var rank in ranks)
        {
            var type = new CardType(suit, rank);
            cards.Add(new Card(new CardId(id++), type));
            cards.Add(new Card(new CardId(id++), type));
        }
        return cards;
    }
}
