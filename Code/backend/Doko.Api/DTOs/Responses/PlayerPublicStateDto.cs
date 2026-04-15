namespace Doko.Api.DTOs.Responses;

public record PlayerPublicStateDto(
    int Id,
    string Seat,
    string? KnownParty,
    int HandCardCount,
    string? HighestAnnouncement
);
