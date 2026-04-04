using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

/// <summary>Elevates ♦ Aces (Füchse) above the Dullen (rank 26 → 28).</summary>
public sealed class SchweinchenModifier : ISonderkarteRankingModifier
{
    public static readonly SchweinchenModifier Instance = new();

    private static readonly CardType KaroAss = new(Suit.Karo, Rank.Ass);

    public bool Applies(CardType card) => card == KaroAss;

    public int ModifiedTrumpRank(CardType card, int baseRank) => 28;
}
