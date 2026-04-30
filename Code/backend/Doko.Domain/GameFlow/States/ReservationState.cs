using Doko.Domain.Hands;
using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Domain.GameFlow;

/// <summary>
/// Reservation discovery cluster: HealthCheck → SoloCheck → ArmutCheck →
/// SchmeissenCheck → HochzeitCheck. <see cref="GameState.Phase"/> discriminates
/// the active sub-phase within this cluster.
/// </summary>
public sealed record ReservationState : GameState
{
    /// <summary>
    /// Round 1 of reservation discovery: each player's health declaration.
    /// True = Vorbehalt (has a reservation), false = Gesund (none).
    /// Null means the player has not yet been asked.
    /// </summary>
    public IReadOnlyDictionary<PlayerSeat, bool> HealthDeclarations { get; init; } =
        new Dictionary<PlayerSeat, bool>();

    /// <summary>
    /// Players still awaiting a declaration in the current reservation check phase
    /// (SoloCheck, ArmutCheck, …). <see cref="GameState.CurrentTurn"/> equals the first entry.
    /// Empty when all players have responded.
    /// </summary>
    public IReadOnlyList<PlayerSeat> PendingReservationResponders { get; init; } = [];

    /// <summary>
    /// Tracks each player's reservation declaration during a check phase.
    /// Populated by <see cref="Modifications.RecordDeclarationModification"/>; null means the player passed.
    /// Cleared between check phases by <see cref="Modifications.ClearReservationDeclarationsModification"/>.
    /// </summary>
    public IReadOnlyDictionary<PlayerSeat, IReservation?> ReservationDeclarations { get; init; } =
        new Dictionary<PlayerSeat, IReservation?>();

    /// <summary>
    /// The player who leads the reservation-check ordering for this round.
    /// Rotates counter-clockwise each game.
    /// </summary>
    public PlayerSeat VorbehaltRauskommer { get; init; }

    /// <summary>
    /// Each player's hand as originally dealt. Set once when dealing completes, never mutated.
    /// </summary>
    public IReadOnlyDictionary<PlayerSeat, Hand>? InitialHands { get; init; }

    /// <summary>The active game mode (reservation). Null for Normalspiel until decided.</summary>
    public IReservation? ActiveReservation { get; init; }

    /// <summary>The player who declared the active game mode (Solo, Hochzeit, Armut player). Null for Normalspiel.</summary>
    public PlayerSeat? GameModePlayerSeat { get; init; }
}
