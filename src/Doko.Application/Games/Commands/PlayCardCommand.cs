using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.Commands;

public record PlayCardCommand(
    GameId GameId,
    PlayerId Player,
    CardId Card,
    IReadOnlyList<SonderkarteType> ActivateSonderkarten);
