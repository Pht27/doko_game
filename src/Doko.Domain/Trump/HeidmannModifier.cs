using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

/// <summary>
/// Heidmann: Jacks rank above Queens. Queens shift down by 8, Jacks shift up by 8,
/// preserving relative order within each group while swapping their bands.
/// </summary>
public sealed class HeidmannModifier : ISonderkarteRankingModifier
{
    public static readonly HeidmannModifier Instance = new();

    public bool Applies(CardType card) => card.Rank is Rank.Dame or Rank.Bube;

    public int ModifiedTrumpRank(CardType card, int baseRank)
        => card.Rank == Rank.Bube ? baseRank + 8 : baseRank - 8;
}
