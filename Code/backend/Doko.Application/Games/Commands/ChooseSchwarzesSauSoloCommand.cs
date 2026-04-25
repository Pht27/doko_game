using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Application.Games.Commands;

/// <summary>
/// The trick winner in <see cref="GamePhase.SchwarzesSauSoloSelect"/> chooses a solo under
/// which the rest of the game is played.
/// </summary>
public record ChooseSchwarzesSauSoloCommand(
    GameId GameId,
    PlayerSeat Player,
    ReservationPriority Solo
);
