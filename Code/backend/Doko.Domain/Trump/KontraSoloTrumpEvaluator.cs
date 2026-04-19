using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

/// <summary>
/// Trump evaluator for Kontrasolo: same as normal, but both ♠ Kings (Klabautermänner)
/// are trump at rank 34 — above all Sonderkarten including Hyperschweinchen (rank 32).
/// </summary>
public sealed class KontraSoloTrumpEvaluator : ITrumpEvaluator
{
    public static readonly KontraSoloTrumpEvaluator Instance = new();

    private static readonly CardType PikKoenig = new(Suit.Pik, Rank.Koenig);

    public bool IsTrump(CardType card) =>
        NormalTrumpEvaluator.Instance.IsTrump(card) || card == PikKoenig;

    public int GetTrumpRank(CardType card)
    {
        if (card == PikKoenig)
            return 34; // Klabautermann — above Hyperschweinchen (32)
        return NormalTrumpEvaluator.Instance.GetTrumpRank(card);
    }

    public int GetPlainRank(CardType card)
    {
        if (card == PikKoenig)
            throw new ArgumentOutOfRangeException(nameof(card), "♠ King is trump in Kontrasolo");
        return NormalTrumpEvaluator.Instance.GetPlainRank(card);
    }
}
