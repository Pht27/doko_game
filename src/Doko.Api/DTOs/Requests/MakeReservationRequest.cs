namespace Doko.Api.DTOs.Requests;

/// <param name="Reservation">ReservationPriority enum name (e.g. "Damensolo"), or null for keine Vorbehalt.</param>
/// <param name="HochzeitCondition">"FirstTrick" | "FirstFehlTrick" | "FirstTrumpTrick" — required when Reservation is "Hochzeit".</param>
/// <param name="ArmutPartner">PlayerId (0–3) of the rich player — required when Reservation is "Armut".</param>
public record MakeReservationRequest(
    string? Reservation,
    string? HochzeitCondition,
    int? ArmutPartner
);
