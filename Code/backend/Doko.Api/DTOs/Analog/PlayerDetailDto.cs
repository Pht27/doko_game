namespace Doko.Api.DTOs.Analog;

public record PlayerDetailDto(
    int Id,
    string Name,
    bool IsActive,
    decimal TotalPoints,
    int GamesPlayed,
    int Wins,
    int Losses,
    IReadOnlyList<PlayerRoundDto> RecentRounds
);
