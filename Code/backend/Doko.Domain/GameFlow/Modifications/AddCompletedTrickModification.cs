namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Appends a completed trick to <c>CompletedTricks</c> and its scored result to <c>ScoredTricks</c>,
/// then clears the current trick.
/// </summary>
public sealed record AddCompletedTrickModification(Tricks.Trick Trick, Scoring.TrickResult Result)
    : GameStateModification;
