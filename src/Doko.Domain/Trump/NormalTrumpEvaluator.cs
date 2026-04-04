using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

public sealed class NormalTrumpEvaluator : ITrumpEvaluator
{
    public static readonly NormalTrumpEvaluator Instance = new();

    private static readonly CardType DulleType = new(Suit.Herz, Rank.Zehn);

    public bool IsTrump(CardType card)
        => card.Rank is Rank.Dame or Rank.Bube
        || card.Suit == Suit.Karo
        || card == DulleType;

    public int GetTrumpRank(CardType card)
    {
        if (card == DulleType) return 26;

        return card switch
        {
            { Rank: Rank.Dame, Suit: Suit.Kreuz }  => 24,
            { Rank: Rank.Dame, Suit: Suit.Pik }    => 22,
            { Rank: Rank.Dame, Suit: Suit.Herz }   => 20,
            { Rank: Rank.Dame, Suit: Suit.Karo }   => 18,
            { Rank: Rank.Bube, Suit: Suit.Kreuz }  => 16,
            { Rank: Rank.Bube, Suit: Suit.Pik }    => 14,
            { Rank: Rank.Bube, Suit: Suit.Herz }   => 12,
            { Rank: Rank.Bube, Suit: Suit.Karo }   => 10,
            { Suit: Suit.Karo, Rank: Rank.Ass }    => 8,
            { Suit: Suit.Karo, Rank: Rank.Koenig } => 6,
            { Suit: Suit.Karo, Rank: Rank.Zehn }   => 4,
            { Suit: Suit.Karo, Rank: Rank.Neun }   => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(card), $"Not a trump card: {card}"),
        };
    }

    public int GetPlainRank(CardType card) => card switch
    {
        { Suit: Suit.Kreuz, Rank: Rank.Ass }    => 4,
        { Suit: Suit.Kreuz, Rank: Rank.Zehn }   => 3,
        { Suit: Suit.Kreuz, Rank: Rank.Koenig } => 2,
        { Suit: Suit.Kreuz, Rank: Rank.Neun }   => 1,
        { Suit: Suit.Pik,   Rank: Rank.Ass }    => 4,
        { Suit: Suit.Pik,   Rank: Rank.Zehn }   => 3,
        { Suit: Suit.Pik,   Rank: Rank.Koenig } => 2,
        { Suit: Suit.Pik,   Rank: Rank.Neun }   => 1,
        { Suit: Suit.Herz,  Rank: Rank.Ass }    => 3,
        { Suit: Suit.Herz,  Rank: Rank.Koenig } => 2,
        { Suit: Suit.Herz,  Rank: Rank.Neun }   => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(card), $"No plain rank for: {card}"),
    };
}
