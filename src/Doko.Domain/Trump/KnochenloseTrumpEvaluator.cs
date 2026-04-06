using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

/// <summary>
/// Trump evaluator for Knochenloses. No trump at all.
/// Plain rank: A > K > Q > J > 10 > 9 (tens are low).
/// </summary>
public sealed class KnochenloseTrumpEvaluator : ITrumpEvaluator
{
    public static readonly KnochenloseTrumpEvaluator Instance = new();

    public bool IsTrump(CardType card) => false;

    public int GetTrumpRank(CardType card) =>
        throw new InvalidOperationException("No trump cards exist in Knochenloses.");

    public int GetPlainRank(CardType card) =>
        card.Rank switch
        {
            Rank.Ass => 6,
            Rank.Koenig => 5,
            Rank.Dame => 4,
            Rank.Bube => 3,
            Rank.Zehn => 2,
            Rank.Neun => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(card), $"No plain rank for: {card}"),
        };
}
