using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

/// <summary>Trump evaluator for Bubensolo. Only Jacks are trump; plain rank is A > 10 > K > Q > 9.</summary>
public sealed class BubensoloTrumpEvaluator : ITrumpEvaluator
{
    public static readonly BubensoloTrumpEvaluator Instance = new();

    public bool IsTrump(CardType card) => card.Rank == Rank.Bube;

    public int GetTrumpRank(CardType card) =>
        card.Suit switch
        {
            Suit.Kreuz => 8,
            Suit.Pik => 6,
            Suit.Herz => 4,
            Suit.Karo => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(card)),
        };

    /// <summary>Plain rank: A > 10 > K > Q > 9 (Queens are below Kings; ♥10 is plain here).</summary>
    public int GetPlainRank(CardType card) =>
        card.Rank switch
        {
            Rank.Ass => 5,
            Rank.Zehn => 4,
            Rank.Koenig => 3,
            Rank.Dame => 2,
            Rank.Neun => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(card), $"No plain rank for: {card}"),
        };
}
