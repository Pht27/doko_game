using Doko.Domain.Hands;
using Doko.Domain.Parties;

namespace Doko.Domain.Players;

public record PlayerState(PlayerId Id, PlayerSeat Seat, Hand Hand, Party? KnownParty);
