using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

/// <summary>
/// Trump evaluator for Fleischloses / Nullo. No trump at all.
/// Plain rank: A > 10 > K > Q > J > 9.
/// </summary>
public sealed class NoTrumpEvaluator : ITrumpEvaluator
{
    public static readonly NoTrumpEvaluator Instance = new();

    public bool IsTrump(CardType card) => false;

    public int GetTrumpRank(CardType card)
        => throw new InvalidOperationException("No trump cards exist in this game mode.");

    public int GetPlainRank(CardType card) => card.Rank switch
    {
        Rank.Ass    => 6,
        Rank.Zehn   => 5,
        Rank.Koenig => 4,
        Rank.Dame   => 3,
        Rank.Bube   => 2,
        Rank.Neun   => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(card), $"No plain rank for: {card}"),
    };
}
