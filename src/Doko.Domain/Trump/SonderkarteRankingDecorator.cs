using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

public abstract class SonderkarteRankingDecorator : ITrumpEvaluator
{
    protected readonly ITrumpEvaluator Inner;
    protected readonly IReadOnlyList<ISonderkarteRankingModifier> Modifiers;

    protected SonderkarteRankingDecorator(
        ITrumpEvaluator inner,
        IReadOnlyList<ISonderkarteRankingModifier> modifiers)
    {
        Inner = inner;
        Modifiers = modifiers;
    }

    public abstract bool IsTrump(CardType card);
    public abstract int GetTrumpRank(CardType card);
    public abstract int GetPlainRank(CardType card);
}
