using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

/// <summary>Elevates ♦ Kings above the Superschweinchen (rank 30 → 32).</summary>
public sealed class HyperschweinchenModifier : ISonderkarteRankingModifier
{
    public static readonly HyperschweinchenModifier Instance = new();

    private static readonly CardType KaroKoenig = new(Suit.Karo, Rank.Koenig);

    public bool Applies(CardType card) => card == KaroKoenig;

    public int ModifiedTrumpRank(CardType card, int baseRank) => 32;
}
