using Doko.Domain.Cards;
using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Rules;
using Doko.Domain.Trump;

namespace Doko.Domain.Reservations;

/// <summary>
/// Farbsolo: same trump structure as normal game but the chosen suit replaces ♦ as the bottom trumps.
/// </summary>
public sealed class FarbsoloReservation : IReservation
{
    private readonly Suit _suit;
    private readonly PlayerSeat _soloPlayer;

    public FarbsoloReservation(Suit suit, PlayerSeat soloPlayer)
    {
        _suit = suit;
        _soloPlayer = soloPlayer;
    }

    public ReservationPriority Priority =>
        _suit switch
        {
            Suit.Karo => ReservationPriority.KaroSolo,
            Suit.Kreuz => ReservationPriority.KreuzSolo,
            Suit.Pik => ReservationPriority.PikSolo,
            Suit.Herz => ReservationPriority.HerzSolo,
            _ => throw new ArgumentOutOfRangeException(nameof(_suit)),
        };

    public bool IsEligible(Hand hand, RuleSet rules) => rules.AllowFarbsoli;

    public GameModeContext Apply() =>
        new(new FarbsoloTrumpEvaluator(_suit), new SoloPartyResolver(_soloPlayer));
}
