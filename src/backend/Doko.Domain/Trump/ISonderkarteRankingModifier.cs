using Doko.Domain.Cards;

namespace Doko.Domain.Trump;

/// <summary>Modifies the trump ranking of specific cards (e.g. Schweinchen, Heidmann).</summary>
public interface ISonderkarteRankingModifier
{
    bool Applies(CardType card);
    int ModifiedTrumpRank(CardType card, int baseRank);
}
