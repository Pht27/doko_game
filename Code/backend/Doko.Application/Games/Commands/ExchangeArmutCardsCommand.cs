using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Application.Games.Commands;

/// <summary>
/// The rich player returns cards to the poor player during <see cref="GamePhase.ArmutCardExchange"/>.
/// </summary>
public record ExchangeArmutCardsCommand(
    GameId GameId,
    PlayerSeat RichPlayer,
    IReadOnlyList<CardId> CardIdsToReturn
);
