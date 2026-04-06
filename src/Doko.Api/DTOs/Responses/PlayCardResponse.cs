namespace Doko.Api.DTOs.Responses;

public record PlayCardResponse(bool TrickCompleted, int? TrickWinner, bool GameFinished, GameResultDto? FinishedResult);
