using Doko.Domain.Players;
using Doko.Domain.Rules;

namespace Doko.Application.Games.Commands;

public record StartGameCommand(IReadOnlyList<PlayerSeat> Players, RuleSet? Rules = null);
