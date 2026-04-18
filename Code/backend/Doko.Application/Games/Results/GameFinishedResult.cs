using Doko.Domain.Scoring;

namespace Doko.Application.Games.Results;

public record GameFinishedResult(GameResult Result, IReadOnlyList<int> NetPointsPerSeat);
