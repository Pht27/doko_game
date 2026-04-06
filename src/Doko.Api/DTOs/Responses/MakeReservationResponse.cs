namespace Doko.Api.DTOs.Responses;

public record MakeReservationResponse(
    bool AllDeclared,
    string? WinningReservation,
    bool Geschmissen
);
