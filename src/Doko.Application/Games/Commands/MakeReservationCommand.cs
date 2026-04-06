using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Application.Games.Commands;

/// <summary>
/// Declares a reservation for a player. <see cref="Reservation"/> is null when the player
/// has no reservation ("keine Vorbehalt").
/// </summary>
public record MakeReservationCommand(GameId GameId, PlayerId Player, IReservation? Reservation);
