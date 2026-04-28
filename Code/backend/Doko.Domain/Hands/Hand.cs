using Doko.Domain.Cards;
using Doko.Domain.Trump;

namespace Doko.Domain.Hands;

public sealed class Hand
{
    public IReadOnlyList<Card> Cards { get; }

    public Hand(IReadOnlyList<Card> cards) => Cards = cards;

    public bool Contains(Card card) => Cards.Contains(card);

    /// <summary>Returns a new Hand with the given card removed. Throws if card is not present.</summary>
    public Hand Remove(Card card)
    {
        var newCards = new List<Card>(Cards);
        bool removed = newCards.Remove(card);
        if (!removed)
            throw new InvalidOperationException($"Card {card} is not in the hand.");
        return new Hand(newCards);
    }

    public IReadOnlyList<Card> SortedFor(ITrumpEvaluator trumpEvaluator) =>
        Cards
            .OrderByDescending(c => trumpEvaluator.IsTrump(c.Type))
            .ThenByDescending(c =>
                trumpEvaluator.IsTrump(c.Type) ? trumpEvaluator.GetTrumpRank(c.Type) : 0
            )
            .ThenBy(c => (int)c.Type.Suit)
            .ThenByDescending(c =>
                trumpEvaluator.IsTrump(c.Type) ? 0 : trumpEvaluator.GetPlainRank(c.Type)
            )
            .ToList();

    public static Hand Empty => new(Array.Empty<Card>());
}
