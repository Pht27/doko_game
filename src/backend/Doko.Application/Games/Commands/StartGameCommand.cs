using Doko.Domain.Players;
using Doko.Domain.Rules;

namespace Doko.Application.Games.Commands;

public record StartGameCommand(IReadOnlyList<PlayerId> Players, RuleSet? Rules = null);
