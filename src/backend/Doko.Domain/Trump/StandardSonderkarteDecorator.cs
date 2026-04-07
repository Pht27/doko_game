using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

/// <summary>
/// Concrete decorator that applies a list of <see cref="ISonderkarteRankingModifier"/>s on top of an
/// inner <see cref="ITrumpEvaluator"/>. A card is trump if the inner evaluator says so, OR if any
/// modifier claims it (e.g. Schweinchen upgrading ♦A above Dullen).
/// </summary>
public sealed class StandardSonderkarteDecorator : SonderkarteRankingDecorator
{
    public StandardSonderkarteDecorator(
        ITrumpEvaluator inner,
        IReadOnlyList<ISonderkarteRankingModifier> modifiers
    )
        : base(inner, modifiers) { }

    public override bool IsTrump(CardType card) =>
        Inner.IsTrump(card) || Modifiers.Any(m => m.Applies(card));

    public override int GetTrumpRank(CardType card)
    {
        int baseRank = Inner.IsTrump(card) ? Inner.GetTrumpRank(card) : 0;
        foreach (var modifier in Modifiers)
        {
            if (modifier.Applies(card))
                baseRank = modifier.ModifiedTrumpRank(card, baseRank);
        }
        return baseRank;
    }

    public override int GetPlainRank(CardType card) => Inner.GetPlainRank(card);
}
