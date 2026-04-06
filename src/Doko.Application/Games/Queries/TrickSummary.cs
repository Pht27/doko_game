using Doko.Domain.Cards;
using Doko.Domain.Players;

namespace Doko.Application.Games.Queries;

public record TrickCardSummary(PlayerId Player, Card Card);

public record TrickSummary(
    int TrickNumber,
    IReadOnlyList<TrickCardSummary> Cards,
    PlayerId? Winner
);
