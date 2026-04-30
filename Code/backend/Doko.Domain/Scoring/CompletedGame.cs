using Doko.Domain.GameFlow;

namespace Doko.Domain.Scoring;

public record CompletedGame(ScoringState FinalState, IReadOnlyList<TrickResult> Tricks);
