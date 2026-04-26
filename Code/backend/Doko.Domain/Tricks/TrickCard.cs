using Doko.Domain.Cards;
using Doko.Domain.Players;

namespace Doko.Domain.Tricks;

public record TrickCard(Card Card, PlayerSeat Player);
