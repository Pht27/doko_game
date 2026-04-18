namespace Doko.Api.DTOs.Responses;

public record ExtrapunktAwardDto(string Type, int BenefittingPlayer, int Delta);

public record GameValueComponentDto(string Label, int Value);

public record AnnouncementRecordDto(string Party, string Type);

public record GameResultDto(
    string Winner,
    int ReAugen,
    int KontraAugen,
    int ReStiche,
    int KontraStiche,
    string? GameMode,
    int GameValue,
    IReadOnlyList<ExtrapunktAwardDto> AllAwards,
    bool Feigheit,
    IReadOnlyList<GameValueComponentDto> ValueComponents,
    int SoloFactor,
    int TotalScore,
    IReadOnlyList<int> NetPointsPerSeat,
    IReadOnlyList<int> LobbyStandings,
    IReadOnlyList<AnnouncementRecordDto> AnnouncementRecords,
    bool IsGeschmissen = false,
    IReadOnlyList<GameResultDto>? MatchHistory = null
);
