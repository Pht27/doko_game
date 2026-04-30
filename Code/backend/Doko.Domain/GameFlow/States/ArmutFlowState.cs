using Doko.Domain.Hands;
using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Domain.GameFlow;

/// <summary>
/// Armut partner-finding and card-exchange cluster.
/// <see cref="GameState.Phase"/> is either <see cref="GamePhase.ArmutPartnerFinding"/>
/// or <see cref="GamePhase.ArmutCardExchange"/>.
/// </summary>
public sealed record ArmutFlowState : GameState
{
    /// <summary>
    /// Players still awaiting a response during partner-finding. Empty during card exchange.
    /// </summary>
    public IReadOnlyList<PlayerSeat> PendingReservationResponders { get; init; } = [];

    /// <summary>
    /// The player who leads the reservation-check ordering for this round.
    /// Carried forward from <see cref="ReservationState"/> for solo/Armut Rauskommer resolution.
    /// </summary>
    public PlayerSeat VorbehaltRauskommer { get; init; }

    /// <summary>
    /// Each player's hand as originally dealt. Set once when dealing completes, never mutated.
    /// </summary>
    public IReadOnlyDictionary<PlayerSeat, Hand>? InitialHands { get; init; }

    /// <summary>The active game mode (Armut reservation).</summary>
    public IReservation? ActiveReservation { get; init; }

    /// <summary>The player who declared Armut.</summary>
    public PlayerSeat? GameModePlayerSeat { get; init; }

    /// <summary>
    /// Armut-phase state. Non-null iff an Armut game mode is active.
    /// </summary>
    public ArmutState? Armut { get; init; }
}
