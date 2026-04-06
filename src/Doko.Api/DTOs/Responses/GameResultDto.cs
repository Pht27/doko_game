namespace Doko.Api.DTOs.Responses;

public record ExtrapunktAwardDto(string Type, int BenefittingPlayer, int Delta);

public record GameResultDto(
    string Winner,
    int RePoints,
    int KontraPoints,
    int GameValue,
    IReadOnlyList<ExtrapunktAwardDto> AllAwards,
    bool Feigheit);
