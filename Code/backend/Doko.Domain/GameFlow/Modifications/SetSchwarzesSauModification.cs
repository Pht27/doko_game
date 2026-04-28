namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Marks the game as Schwarze Sau (Armut with no partner found). From this point the game
/// watches for the second ♠Q trick and then interrupts with
/// <see cref="GamePhase.SchwarzesSauSoloSelect"/>.
/// </summary>
public sealed record SetSchwarzesSauModification : GameStateModification;
