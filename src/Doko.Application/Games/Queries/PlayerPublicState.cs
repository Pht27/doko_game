using Doko.Domain.Parties;
using Doko.Domain.Players;

namespace Doko.Application.Games.Queries;

/// <summary>What one player can see about another player's public state.</summary>
public record PlayerPublicState(
    PlayerId Id,
    PlayerSeat Seat,
    Party? KnownParty,
    int HandCardCount);
