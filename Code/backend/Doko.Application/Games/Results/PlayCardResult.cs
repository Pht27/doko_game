using Doko.Domain.Players;

namespace Doko.Application.Games.Results;

public record PlayCardResult(
    bool TrickCompleted,
    PlayerSeat? TrickWinner,
    bool GameFinished,
    GameFinishedResult? FinishedResult
);
