using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>Sets the player who declared Armut.</summary>
public sealed record SetArmutPlayerModification(PlayerSeat ArmutPlayer) : GameStateModification;
