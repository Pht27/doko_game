namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Clears all active sonderkarte state (active list and closed windows).
/// Applied in <see cref="GamePhase.SchwarzesSauSoloSelect"/> when a non-Schlanker-Martin
/// solo is chosen: the new trump evaluator makes previously-active sonderkarten irrelevant.
/// </summary>
public sealed record ClearActiveSonderkartenModification : GameStateModification;
