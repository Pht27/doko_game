using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.Commands;

public record PlayCardCommand(
    GameId GameId,
    PlayerSeat Player,
    CardId Card,
    IReadOnlyList<SonderkarteType> ActivateSonderkarten,
    /// <summary>
    /// Required when the player activates Genscherdamen or Gegengenscherdamen.
    /// The named player becomes the Genscher's Re partner.
    /// </summary>
    PlayerSeat? GenscherPartner = null
);
