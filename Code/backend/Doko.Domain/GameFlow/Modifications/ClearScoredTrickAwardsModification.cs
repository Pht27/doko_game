namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Strips all extrapunkt awards from every already-scored trick.
/// Applied when a Schwarze-Sau solo is chosen: extrapunkte earned during the preceding
/// Normalspiel phase are invalidated. Future tricks will accumulate awards under the
/// new solo's extrapunkt rules.
/// </summary>
public sealed record ClearScoredTrickAwardsModification : GameStateModification;
