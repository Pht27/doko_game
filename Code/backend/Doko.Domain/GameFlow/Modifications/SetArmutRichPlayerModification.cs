using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>Sets the rich player who accepted the Armut.</summary>
public sealed record SetArmutRichPlayerModification(PlayerSeat RichPlayer) : GameStateModification;
