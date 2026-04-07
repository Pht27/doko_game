using Doko.Domain.Reservations;

namespace Doko.Application.Games.Results;

public record MakeReservationResult(
    bool AllDeclared,
    IReservation? WinningReservation,
    bool Geschmissen = false
);
