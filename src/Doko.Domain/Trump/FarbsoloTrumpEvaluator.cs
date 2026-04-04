using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

/// <summary>Trump evaluator for Farbsoli. The chosen suit replaces ♦ as the bottom trump suit.</summary>
public sealed class FarbsoloTrumpEvaluator : ITrumpEvaluator
{
    private readonly Suit _chosenSuit;
    private static readonly CardType DulleType = new(Suit.Herz, Rank.Zehn);

    public FarbsoloTrumpEvaluator(Suit chosenSuit) => _chosenSuit = chosenSuit;

    public bool IsTrump(CardType card)
        => card.Rank is Rank.Dame or Rank.Bube
        || card == DulleType
        || card.Suit == _chosenSuit;

    public int GetTrumpRank(CardType card)
    {
        if (card == DulleType) return 26;

        if (card.Rank is Rank.Dame or Rank.Bube)
        {
            return (card.Rank, card.Suit) switch
            {
                (Rank.Dame, Suit.Kreuz) => 24,
                (Rank.Dame, Suit.Pik)   => 22,
                (Rank.Dame, Suit.Herz)  => 20,
                (Rank.Dame, Suit.Karo)  => 18,
                (Rank.Bube, Suit.Kreuz) => 16,
                (Rank.Bube, Suit.Pik)   => 14,
                (Rank.Bube, Suit.Herz)  => 12,
                (Rank.Bube, Suit.Karo)  => 10,
                _ => throw new ArgumentOutOfRangeException(nameof(card)),
            };
        }

        // Chosen suit bottom trumps: A > K > 10 > 9
        return card.Rank switch
        {
            Rank.Ass    => 8,
            Rank.Koenig => 6,
            Rank.Zehn   => 4,
            Rank.Neun   => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(card), $"Not a trump card: {card}"),
        };
    }

    /// <summary>Plain rank within a non-trump suit: A > 10 > K > 9.</summary>
    public int GetPlainRank(CardType card) => card.Rank switch
    {
        Rank.Ass    => 4,
        Rank.Zehn   => 3,
        Rank.Koenig => 2,
        Rank.Neun   => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(card), $"No plain rank for: {card}"),
    };
}
