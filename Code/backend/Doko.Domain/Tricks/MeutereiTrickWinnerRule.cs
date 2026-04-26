using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Tricks;

/// <summary>
/// If the first or second card is a ♠K, followed by a ♠Q (attempt to capture Klabautermann),
/// followed by a second ♠K — and the ♠Q would normally win the trick — the trick goes to
/// the player of the second ♠K instead.
/// </summary>
public sealed class MeutereiTrickWinnerRule : ITrickWinnerRule
{
    private static readonly CardType PikKoenig = new(Suit.Pik, Rank.Koenig);
    private static readonly CardType PikDame = new(Suit.Pik, Rank.Dame);

    public PlayerSeat? TryGetOverride(
        Trick completedTrick,
        GameState state,
        PlayerSeat normalWinner
    )
    {
        var cards = completedTrick.Cards;

        // First ♠K must be at position 0 or 1
        int firstKoenigIdx = -1;
        for (int i = 0; i <= 1; i++)
        {
            if (cards[i].Card.Type == PikKoenig)
            {
                firstKoenigIdx = i;
                break;
            }
        }
        if (firstKoenigIdx < 0)
            return null;

        // ♠Q must appear after the first ♠K
        int dameIdx = -1;
        for (int i = firstKoenigIdx + 1; i < cards.Count; i++)
        {
            if (cards[i].Card.Type == PikDame)
            {
                dameIdx = i;
                break;
            }
        }
        if (dameIdx < 0)
            return null;

        // Second ♠K must appear after the ♠Q
        int secondKoenigIdx = -1;
        for (int i = dameIdx + 1; i < cards.Count; i++)
        {
            if (cards[i].Card.Type == PikKoenig)
            {
                secondKoenigIdx = i;
                break;
            }
        }
        if (secondKoenigIdx < 0)
            return null;

        // ♠Q must be the highest card (normal winner)
        if (normalWinner != cards[dameIdx].Player)
            return null;

        return cards[secondKoenigIdx].Player;
    }
}
