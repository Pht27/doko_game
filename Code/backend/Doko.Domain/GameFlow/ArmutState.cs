using Doko.Domain.Players;

namespace Doko.Domain.GameFlow;

public sealed record ArmutState(
    PlayerSeat Player,
    PlayerSeat? RichPlayer,
    int TransferCount,
    bool? ReturnedTrump
);
