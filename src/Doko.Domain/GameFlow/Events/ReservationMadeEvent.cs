using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Domain.GameFlow.Events;

public sealed record ReservationMadeEvent(
    GameId GameId,
    PlayerId Player,
    IReservation? Reservation) : IDomainEvent;
