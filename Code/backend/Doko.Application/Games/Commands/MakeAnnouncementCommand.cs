using Doko.Domain.Announcements;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Application.Games.Commands;

public record MakeAnnouncementCommand(GameId GameId, PlayerSeat Player, AnnouncementType Type);
