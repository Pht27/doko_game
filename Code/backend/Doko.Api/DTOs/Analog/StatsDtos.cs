namespace Doko.Api.DTOs.Analog;

public record PlayerStatsDto(
    int PlayerId,
    string Name,
    bool IsActive,
    int TotalGames,
    int TotalWins,
    decimal TotalWinRate,
    decimal TotalAvgGameValue,
    decimal TotalAvgPointsWonLost,
    decimal TotalAvgPointsEarned,
    int SoloGames,
    int SoloWins,
    decimal SoloWinRate,
    decimal SoloAvgGameValue,
    decimal SoloAvgPointsWonLost,
    int AloneGames,
    int AloneWins,
    decimal AloneWinRate,
    decimal AloneAvgPointsEarned
);

public record PlayerGameModeStatsDto(
    int GameModeId,
    string GameModeName,
    int Party,
    int Games,
    int Wins,
    decimal WinRate,
    decimal AvgGameValue,
    decimal AvgPointsWonLost,
    decimal AvgPointsEarned
);

public record PlayerSpecialCardStatsDto(
    int SpecialCardId,
    string SpecialCardName,
    int Occurrences,
    int Wins,
    decimal WinRate,
    decimal AvgGameValue,
    decimal AvgPointsWonLost
);

public record PlayerExtraPointStatsDto(
    int ExtraPointId,
    string ExtraPointName,
    int Occurrences,
    int TotalCount,
    int Wins,
    decimal WinRate,
    decimal AvgGameValue
);

public record PlayerPartnerStatsDto(
    int PartnerId,
    string PartnerName,
    int GamesTogether,
    int WinsTogether,
    decimal WinRateTogether,
    decimal AvgPointsWonLost
);

public record GameModeStatsDto(
    int GameModeId,
    string GameModeName,
    int TotalRounds,
    decimal AvgGameValue,
    decimal? ReWinRate,
    decimal? ReAvgGameValue
);

public record SpecialCardStatsDto(
    int SpecialCardId,
    string Name,
    int Occurrences,
    int Wins,
    decimal WinRate,
    decimal AvgGameValue,
    decimal AvgPointsWonLost
);

public record ExtraPointStatsDto(
    int ExtraPointId,
    string Name,
    int Occurrences,
    int TotalCount,
    int Wins,
    decimal WinRate,
    decimal AvgGameValue,
    decimal AvgPointsWonLost
);
