namespace Doko.Domain.GameFlow;

/// <summary>
/// Reservation discovery cluster: HealthCheck → SoloCheck → ArmutCheck →
/// SchmeissenCheck → HochzeitCheck. <see cref="GameState.Phase"/> discriminates
/// the active sub-phase within this cluster.
/// </summary>
public sealed record ReservationState : GameState { }
