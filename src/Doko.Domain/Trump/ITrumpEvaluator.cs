using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

public interface ITrumpEvaluator
{
    bool IsTrump(CardType card);

    /// <summary>Returns the trump ranking of the card (higher = stronger). Only valid if IsTrump returns true.</summary>
    int GetTrumpRank(CardType card);

    /// <summary>Returns the plain-suit ranking of the card (higher = stronger). Only valid if IsTrump returns false.</summary>
    int GetPlainRank(CardType card);
}
