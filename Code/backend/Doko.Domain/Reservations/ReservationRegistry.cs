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
        PlayerSeat player,
        Hand hand,
        RuleSet rules
    )
    {
        var dummyPartner = (PlayerSeat)(((int)player + 1) % 4);

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

    /// <summary>Returns eligible Solo reservation priorities (excludes Armut, Hochzeit, Schmeißen, Schlanker Martin).</summary>
    public static IReadOnlyList<ReservationPriority> GetEligibleSolos(
        PlayerSeat player,
        Hand hand,
        RuleSet rules
    )
    {
        IReservation[] solos =
        [
            new FarbsoloReservation(Suit.Karo, player),
            new FarbsoloReservation(Suit.Kreuz, player),
            new FarbsoloReservation(Suit.Pik, player),
            new FarbsoloReservation(Suit.Herz, player),
            new DamensoloReservation(player),
            new BubensoloReservation(player),
            new FleischlosesReservation(player),
            new KnochenlosesReservation(player),
        ];

        return solos.Where(r => r.IsEligible(hand, rules)).Select(r => r.Priority).ToList();
    }

    /// <summary>Returns Armut if eligible, otherwise empty.</summary>
    public static IReadOnlyList<ReservationPriority> GetEligibleArmut(
        PlayerSeat player,
        Hand hand,
        RuleSet rules
    )
    {
        var dummyPartner = (PlayerSeat)(((int)player + 1) % 4);
        var armut = new ArmutReservation(player, dummyPartner);
        return armut.IsEligible(hand, rules) ? [ReservationPriority.Armut] : [];
    }

    /// <summary>Returns Schmeißen if eligible, otherwise empty.</summary>
    public static IReadOnlyList<ReservationPriority> GetEligibleSchmeissen(
        PlayerSeat player,
        Hand hand,
        RuleSet rules
    )
    {
        var schmeissen = new SchmeissenReservation();
        return schmeissen.IsEligible(hand, rules) ? [ReservationPriority.Schmeissen] : [];
    }

    /// <summary>Returns Hochzeit if eligible, otherwise empty.</summary>
    public static IReadOnlyList<ReservationPriority> GetEligibleHochzeit(
        PlayerSeat player,
        Hand hand,
        RuleSet rules
    )
    {
        var hochzeit = new HochzeitReservation(player, HochzeitCondition.FirstTrick);
        return hochzeit.IsEligible(hand, rules) ? [ReservationPriority.Hochzeit] : [];
    }

    /// <summary>Constructs the concrete <see cref="IReservation"/> for the given priority and player.</summary>
    public static IReservation CreateForPriority(ReservationPriority priority, PlayerSeat player) =>
        priority switch
        {
            ReservationPriority.KaroSolo => new FarbsoloReservation(Suit.Karo, player),
            ReservationPriority.KreuzSolo => new FarbsoloReservation(Suit.Kreuz, player),
            ReservationPriority.PikSolo => new FarbsoloReservation(Suit.Pik, player),
            ReservationPriority.HerzSolo => new FarbsoloReservation(Suit.Herz, player),
            ReservationPriority.Damensolo => new DamensoloReservation(player),
            ReservationPriority.Bubensolo => new BubensoloReservation(player),
            ReservationPriority.Fleischloses => new FleischlosesReservation(player),
            ReservationPriority.Knochenloses => new KnochenlosesReservation(player),
            ReservationPriority.SchlankerMartin => new SchlankerMartinReservation(player),
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
        };
}
