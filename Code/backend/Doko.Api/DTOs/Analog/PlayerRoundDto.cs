namespace Doko.Api.DTOs.Analog;

public record PlayerRoundDto(
    int RoundId,
    DateTime PlayedAt,
    decimal Points,
    bool Won,
    decimal CumulativePoints
);
