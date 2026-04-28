namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Signals that the trump evaluator must be rebuilt.
/// Returned from <see cref="ISonderkarte"/> by sonderkarten that affect trump order
/// (e.g. Schweinchen, Heidmann, Heidfrau). The rebuild reads ranking modifiers and
/// suppressions directly from <see cref="GameState.ActiveSonderkarten"/>.
/// Sonderkarten without trump effects (Kemmerich, Genscherdamen, …) do not return this.
/// </summary>
public sealed record RebuildTrumpEvaluatorModification : GameStateModification;
