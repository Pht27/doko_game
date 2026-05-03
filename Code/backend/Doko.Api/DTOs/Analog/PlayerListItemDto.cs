namespace Doko.Api.DTOs.Analog;

public record PlayerListItemDto(
    int Id,
    string Name,
    bool IsActive,
    decimal TotalPoints,
    int GamesPlayed,
    int Wins,
    int Losses
);
