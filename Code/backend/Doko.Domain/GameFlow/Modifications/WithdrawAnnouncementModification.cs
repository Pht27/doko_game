using Doko.Domain.Announcements;
using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Modifications;

public sealed record WithdrawAnnouncementModification(PlayerSeat Player, AnnouncementType Type)
    : GameStateModification;
