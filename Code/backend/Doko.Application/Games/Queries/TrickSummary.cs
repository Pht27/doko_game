using Doko.Domain.Cards;
using Doko.Domain.Players;

namespace Doko.Application.Games.Queries;

public record TrickCardSummary(PlayerSeat Player, Card Card, bool FaceDown = false);

public record TrickSummary(
    int TrickNumber,
    IReadOnlyList<TrickCardSummary> Cards,
    PlayerSeat? Winner
);
