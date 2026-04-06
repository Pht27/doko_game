namespace Doko.Api.DTOs.Responses;

public record TrickCardDto(int Player, CardDto Card);

public record TrickSummaryDto(int TrickNumber, IReadOnlyList<TrickCardDto> Cards, int? Winner);
