using Doko.Domain.GameFlow;

namespace Doko.Domain.Scoring;

public record CompletedGame(
    GameState FinalState,
    IReadOnlyList<TrickResult> Tricks);
