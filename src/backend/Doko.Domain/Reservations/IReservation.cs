using Doko.Domain.Hands;
using Doko.Domain.Rules;

namespace Doko.Domain.Reservations;

public interface IReservation
{
    ReservationPriority Priority { get; }

    /// <summary>True for solo reservations (1 player vs 3). Priority 0–8 are all Soli.</summary>
    bool IsSolo => Priority <= ReservationPriority.SchlankerMartin;

    bool IsEligible(Hand hand, RuleSet rules);
    GameModeContext Apply();
}
