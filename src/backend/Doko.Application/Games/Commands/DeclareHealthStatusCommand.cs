using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Application.Games.Commands;

/// <summary>Declares a player's health status in the <see cref="GamePhase.ReservationHealthCheck"/> phase.</summary>
public record DeclareHealthStatusCommand(GameId GameId, PlayerId Player, bool HasVorbehalt);
