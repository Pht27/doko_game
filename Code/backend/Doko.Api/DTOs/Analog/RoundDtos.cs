using Doko.Analog;

namespace Doko.Api.DTOs.Analog;

public record RoundRequest(
    DateTime PlayedAt,
    Party WinningParty,
    int Points,
    int GameModeId,
    TeamRequest[] Teams,
    string? Comment
);

public record TeamRequest(
    Party Party,
    int[] PlayerIds,
    int[] SpecialCardIds,
    ExtraPointRequest[] ExtraPoints
);

public record ExtraPointRequest(int ExtraPointId, int Count);

public record RoundListResponse(int Total, int Page, RoundListItemDto[] Items);

public record RoundListItemDto(
    int Id,
    DateTime PlayedAt,
    Party WinningParty,
    int Points,
    string GameMode,
    PlayerInfoDto[] RePlayers,
    PlayerInfoDto[] KontraPlayers,
    string? Comment
);

public record RoundDetailDto(
    int Id,
    DateTime PlayedAt,
    Party WinningParty,
    int Points,
    GameModeInfoDto GameMode,
    RoundTeamDetailDto[] Teams,
    string? Comment
);

public record RoundTeamDetailDto(
    Party Party,
    PlayerInfoDto[] Players,
    SpecialCardInfoDto[] SpecialCards,
    ExtraPointEntryDto[] ExtraPoints
);

public record GameModeInfoDto(int Id, string Name, bool IsSolo);

public record PlayerInfoDto(int Id, string Name);

public record SpecialCardInfoDto(int Id, string Name);

public record ExtraPointEntryDto(int Id, string Name, int Count);
