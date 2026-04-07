using Doko.Domain.Players;

namespace Doko.Application.Games.Results;

public record PlayCardResult(
    bool TrickCompleted,
    PlayerId? TrickWinner,
    bool GameFinished,
    GameFinishedResult? FinishedResult
);
