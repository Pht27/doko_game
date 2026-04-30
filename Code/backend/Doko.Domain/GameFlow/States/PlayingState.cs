namespace Doko.Domain.GameFlow;

/// <summary>
/// Active play cluster. <see cref="GameState.Phase"/> is either
/// <see cref="GamePhase.Playing"/> or <see cref="GamePhase.SchwarzesSauSoloSelect"/>.
/// </summary>
public sealed record PlayingState : GameState { }
