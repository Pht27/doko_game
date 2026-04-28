namespace Doko.Domain.GameFlow.Modifications;

/// <summary>Advances the game to a new phase.</summary>
public sealed record AdvancePhaseModification(GamePhase NewPhase) : GameStateModification;
