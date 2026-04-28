using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>Sets whose turn it is.</summary>
public sealed record SetCurrentTurnModification(PlayerSeat Player) : GameStateModification;
