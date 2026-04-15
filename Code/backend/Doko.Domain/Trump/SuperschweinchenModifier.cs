using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

/// <summary>Elevates ♦ 10s above the Schweinchen (rank 28 → 30).</summary>
public sealed class SuperschweinchenModifier : ISonderkarteRankingModifier
{
    public static readonly SuperschweinchenModifier Instance = new();

    private static readonly CardType KaroZehn = new(Suit.Karo, Rank.Zehn);

    public bool Applies(CardType card) => card == KaroZehn;

    public int ModifiedTrumpRank(CardType card, int baseRank) => 30;
}
