using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Resolves the game mode after reservations: sets the active reservation (null = normal game),
/// and rebuilds the trump evaluator and party resolver accordingly.
/// </summary>
public sealed record SetGameModeModification(IReservation? Reservation, PlayerSeat? Player = null)
    : GameStateModification;
