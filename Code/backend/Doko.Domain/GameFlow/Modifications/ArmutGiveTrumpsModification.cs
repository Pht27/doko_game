using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Transfers all trump cards from the poor player's hand to the rich player's hand and
/// records the transfer count in <c>ArmutTransferCount</c>.
/// </summary>
public sealed record ArmutGiveTrumpsModification(PlayerSeat PoorPlayer, PlayerSeat RichPlayer)
    : GameStateModification;
