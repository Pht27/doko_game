namespace Doko.Api.DTOs.Analog;

public record StaticDataDto(
    IReadOnlyList<GameModeDto> GameModes,
    IReadOnlyList<SpecialCardDto> SpecialCards,
    IReadOnlyList<ExtraPointDto> ExtraPoints
);

public record GameModeDto(int Id, string Name, bool IsSolo);

public record SpecialCardDto(int Id, string Name);

public record ExtraPointDto(int Id, string Name);
