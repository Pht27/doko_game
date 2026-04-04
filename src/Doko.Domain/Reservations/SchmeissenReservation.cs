using Doko.Domain.Cards;
using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Rules;
using Doko.Domain.Trump;

namespace Doko.Domain.Reservations;

/// <summary>
/// Schmeißen: player may force a redeal if their hand meets any qualifying condition.
/// Apply() returns a normal-game context; the actual redeal is handled by the game engine.
/// </summary>
public sealed class SchmeissenReservation : IReservation
{
    public ReservationPriority Priority => ReservationPriority.Schmeissen;

    public bool IsEligible(Hand hand, RuleSet rules)
        => rules.AllowSchmeissen && QualifiesForSchmeissen(hand);

    public GameModeContext Apply()
        => new(NormalTrumpEvaluator.Instance, NormalPartyResolver.Instance);

    private static bool QualifiesForSchmeissen(Hand hand)
    {
        int totalPoints = hand.Cards.Sum(c => CardPoints.Of(c.Type.Rank));
        if (totalPoints > 80 || totalPoints < 35) return true;

        int trumpCount = hand.Cards.Count(c => IsNormalTrump(c.Type));
        if (trumpCount <= 3) return true;

        if (HighestTrumpIsKaroBube(hand)) return true;

        int nineCount  = hand.Cards.Count(c => c.Type.Rank == Rank.Neun);
        int kingCount  = hand.Cards.Count(c => c.Type.Rank == Rank.Koenig);
        if (nineCount >= 5 || kingCount >= 5 || nineCount + kingCount >= 8) return true;

        return false;
    }

    private static bool IsNormalTrump(CardType c)
        => c.Rank is Rank.Dame or Rank.Bube
        || (c.Suit == Suit.Herz && c.Rank == Rank.Zehn)
        || c.Suit == Suit.Karo;

    private static bool HighestTrumpIsKaroBube(Hand hand)
    {
        var trumpCards = hand.Cards.Where(c => IsNormalTrump(c.Type)).ToList();
        if (trumpCards.Count == 0) return false;
        // ♦J has rank 10 in NormalTrumpEvaluator
        int maxRank = trumpCards.Max(c => NormalTrumpEvaluator.Instance.GetTrumpRank(c.Type));
        return maxRank == 10;
    }
}
