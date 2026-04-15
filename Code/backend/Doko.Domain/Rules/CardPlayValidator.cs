using Doko.Domain.Cards;
using Doko.Domain.Hands;
using Doko.Domain.Tricks;
using Doko.Domain.Trump;

namespace Doko.Domain.Rules;

public static class CardPlayValidator
{
    /// <summary>
    /// Returns true if the player is allowed to play <paramref name="card"/> from their hand.
    /// Enforces the Bedienen (follow-suit) rule:
    /// — if the led card is trump, the player must play trump if they have any;
    /// — if the led card is a plain suit, the player must play that same plain suit if they have any;
    /// — if unable to follow suit, any card may be played.
    /// </summary>
    public static bool CanPlay(
        Card card,
        Hand hand,
        Trick currentTrick,
        ITrumpEvaluator trumpEvaluator
    )
    {
        if (currentTrick.Cards.Count == 0)
            return true;

        var ledCard = currentTrick.Cards[0].Card;
        bool ledIsTrump = trumpEvaluator.IsTrump(ledCard.Type);

        if (ledIsTrump)
        {
            bool handHasTrump = hand.Cards.Any(c => trumpEvaluator.IsTrump(c.Type));
            if (!handHasTrump)
                return true;
            return trumpEvaluator.IsTrump(card.Type);
        }
        else
        {
            // Plain lead: must follow the same plain suit (excluding trump cards of that suit)
            Suit ledSuit = ledCard.Type.Suit;
            bool handHasLedSuit = hand.Cards.Any(c =>
                !trumpEvaluator.IsTrump(c.Type) && c.Type.Suit == ledSuit
            );
            if (!handHasLedSuit)
                return true;
            return !trumpEvaluator.IsTrump(card.Type) && card.Type.Suit == ledSuit;
        }
    }
}
