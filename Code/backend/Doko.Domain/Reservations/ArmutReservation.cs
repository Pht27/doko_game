using Doko.Domain.Cards;
using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Rules;
using Doko.Domain.Trump;

namespace Doko.Domain.Reservations;

/// <summary>
/// Armut: player holds ≤ 3 trump cards (♦ Aces / Füchse excluded from count).
/// All Sonderkarten are deactivated in Armut. The poor and rich players form the Re party.
/// </summary>
public sealed class ArmutReservation : IReservation
{
    private readonly PlayerSeat _poorPlayer;
    private readonly PlayerSeat _richPlayer;

    public ArmutReservation(PlayerSeat poorPlayer, PlayerSeat richPlayer)
    {
        _poorPlayer = poorPlayer;
        _richPlayer = richPlayer;
    }

    public ReservationPriority Priority => ReservationPriority.Armut;

    public bool IsEligible(Hand hand, RuleSet rules) =>
        rules.AllowArmut && CountArmutTrump(hand) <= 3;

    public GameModeContext Apply() =>
        new(NormalTrumpEvaluator.Instance, new ArmutPartyResolver(_poorPlayer, _richPlayer));

    /// <summary>Counts trump for Armut eligibility: excludes ♦ Aces (Füchse).</summary>
    private static int CountArmutTrump(Hand hand) => hand.Cards.Count(c => IsArmutTrump(c.Type));

    private static bool IsArmutTrump(CardType c) =>
        c.Rank is Rank.Dame or Rank.Bube
        || c.IsDulle()
        || (c.Suit == Suit.Karo && c.Rank != Rank.Ass); // ♦A excluded
}
