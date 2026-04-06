using Doko.Domain.Cards;
using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Rules;

namespace Doko.Domain.Reservations;

/// <summary>
/// Central registry of all reservation types. Determines which reservations a player
/// may declare given their hand and the active rule set.
/// </summary>
public static class ReservationRegistry
{
    /// <summary>
    /// Returns the priorities of all reservations the player is eligible to declare.
    /// Ordered by priority (highest first). "Keine Vorbehalt" is always implicitly available
    /// and is not included in the result.
    /// </summary>
    public static IReadOnlyList<ReservationPriority> GetEligible(
        PlayerId player,
        Hand hand,
        RuleSet rules
    )
    {
        var dummyPartner = new PlayerId((byte)((player.Value + 1) % 4));

        IReservation[] all =
        [
            new FarbsoloReservation(Suit.Karo, player),
            new FarbsoloReservation(Suit.Kreuz, player),
            new FarbsoloReservation(Suit.Pik, player),
            new FarbsoloReservation(Suit.Herz, player),
            new DamensoloReservation(player),
            new BubensoloReservation(player),
            new FleischlosesReservation(player),
            new KnochenlosesReservation(player),
            new SchlankerMartinReservation(player),
            new ArmutReservation(player, dummyPartner),
            new HochzeitReservation(player, HochzeitCondition.FirstTrick),
            new SchmeissenReservation(),
        ];

        return all.Where(r => r.IsEligible(hand, rules)).Select(r => r.Priority).ToList();
    }
}
