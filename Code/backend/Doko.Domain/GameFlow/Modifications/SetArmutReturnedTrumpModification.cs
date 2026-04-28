namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Records whether the rich player's returned cards included any trump during
/// <see cref="GamePhase.ArmutCardExchange"/>. Used to display the exchange announcement.
/// </summary>
public sealed record SetArmutReturnedTrumpModification(bool IncludedTrump) : GameStateModification;
