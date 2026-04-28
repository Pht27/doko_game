using Doko.Domain.Cards;
using Doko.Domain.Players;
using Doko.Domain.Trump;

namespace Doko.Domain.Tricks;

public sealed class Trick
{
    private readonly List<TrickCard> _cards = new();

    public IReadOnlyList<TrickCard> Cards => _cards;

    public bool IsComplete => _cards.Count == 4;

    /// <summary>Sum of card points for all cards in this trick.</summary>
    public int Points => _cards.Sum(tc => CardPoints.Of(tc.Card.Type.Rank));

    public void Add(TrickCard trickCard) => _cards.Add(trickCard);

    /// <summary>Returns the winner of this trick. Only valid when IsComplete is true.</summary>
    /// <param name="secondBeatsFirst">
    /// When true, any tie between equal-ranked cards is won by the later card (Schlanker Martin).
    /// Applies to plain-suit ties and non-Dulle trump ties; Dulle ties are governed by <paramref name="dulleRule"/>.
    /// </param>
    public PlayerSeat Winner(
        ITrumpEvaluator trumpEvaluator,
        Rules.DulleRule dulleRule,
        bool secondBeatsFirst = false
    )
    {
        var led = _cards[0];
        bool winnerIsTrump = trumpEvaluator.IsTrump(led.Card.Type);
        PlayerSeat winner = led.Player;
        CardType winnerType = led.Card.Type;
        int winnerRank = winnerIsTrump
            ? trumpEvaluator.GetTrumpRank(led.Card.Type)
            : trumpEvaluator.GetPlainRank(led.Card.Type);

        for (int i = 1; i < _cards.Count; i++)
        {
            var tc = _cards[i];
            bool isTrump = trumpEvaluator.IsTrump(tc.Card.Type);

            if (!isTrump)
            {
                // Can only beat a plain lead of the same suit when no trump has been played yet
                if (!winnerIsTrump && tc.Card.Type.Suit == led.Card.Type.Suit)
                {
                    int rank = trumpEvaluator.GetPlainRank(tc.Card.Type);
                    if (rank > winnerRank || (rank == winnerRank && secondBeatsFirst))
                    {
                        winner = tc.Player;
                        winnerType = tc.Card.Type;
                        winnerRank = rank;
                    }
                }
                continue;
            }

            int trumpRank = trumpEvaluator.GetTrumpRank(tc.Card.Type);

            if (!winnerIsTrump)
            {
                // First trump overrides any plain card
                winner = tc.Player;
                winnerType = tc.Card.Type;
                winnerIsTrump = true;
                winnerRank = trumpRank;
                continue;
            }

            // Both trump: handle Dulle tie-break then normal comparison
            if (tc.Card.Type.IsDulle() && winnerType.IsDulle())
            {
                if (dulleRule == Rules.DulleRule.SecondBeatsFirst)
                {
                    winner = tc.Player;
                    winnerType = tc.Card.Type;
                }
                // FirstBeatsSecond: first wins — no change
            }
            else if (trumpRank > winnerRank || (trumpRank == winnerRank && secondBeatsFirst))
            {
                winner = tc.Player;
                winnerType = tc.Card.Type;
                winnerRank = trumpRank;
            }
        }

        return winner;
    }
}
