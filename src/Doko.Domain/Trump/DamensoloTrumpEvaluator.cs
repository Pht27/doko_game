using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

/// <summary>Trump evaluator for Damensolo. Only Queens are trump; plain rank is A > 10 > K > J > 9.</summary>
public sealed class DamensoloTrumpEvaluator : ITrumpEvaluator
{
    public static readonly DamensoloTrumpEvaluator Instance = new();

    public bool IsTrump(CardType card) => card.Rank == Rank.Dame;

    public int GetTrumpRank(CardType card) => card.Suit switch
    {
        Suit.Kreuz => 8,
        Suit.Pik   => 6,
        Suit.Herz  => 4,
        Suit.Karo  => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(card)),
    };

    /// <summary>Plain rank: A > 10 > K > J > 9 (Jacks are below Kings; ♥10 is plain here).</summary>
    public int GetPlainRank(CardType card) => card.Rank switch
    {
        Rank.Ass    => 5,
        Rank.Zehn   => 4,
        Rank.Koenig => 3,
        Rank.Bube   => 2,
        Rank.Neun   => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(card), $"No plain rank for: {card}"),
    };
}
