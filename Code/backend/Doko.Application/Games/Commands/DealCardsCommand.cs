using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Application.Games.Commands;

public record DealCardsCommand(GameId GameId, PlayerId? VorbehaltRauskommer = null);
