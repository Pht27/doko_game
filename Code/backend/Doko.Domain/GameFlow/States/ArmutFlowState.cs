namespace Doko.Domain.GameFlow;

/// <summary>
/// Armut partner-finding and card-exchange cluster.
/// <see cref="GameState.Phase"/> is either <see cref="GamePhase.ArmutPartnerFinding"/>
/// or <see cref="GamePhase.ArmutCardExchange"/>.
/// </summary>
public sealed record ArmutFlowState : GameState { }
