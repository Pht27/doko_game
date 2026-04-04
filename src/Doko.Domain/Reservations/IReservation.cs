using Doko.Domain.Hands;
using Doko.Domain.Rules;

namespace Doko.Domain.Reservations;

public interface IReservation
{
    ReservationPriority Priority { get; }
    bool IsEligible(Hand hand, RuleSet rules);
    GameModeContext Apply();
}
