using Doko.Domain.Parties;
using Doko.Domain.Players;

namespace Doko.Application.Games.Queries;

/// <summary>What one player can see about another player's public state.</summary>
public record PlayerPublicState(
    PlayerSeat Seat,
    Party? KnownParty,
    int HandCardCount,
    /// <summary>
    /// The most specific announcement this player has made, if any.
    /// "Re" or "Kontra" for <see cref="AnnouncementType.Win"/>; enum name otherwise.
    /// Null when the player has not announced.
    /// </summary>
    string? HighestAnnouncement = null,
    /// <summary>
    /// "Gesund" or "Vorbehalt" once this player has declared during ReservationHealthCheck.
    /// Null before they have declared or outside reservation phases.
    /// </summary>
    string? HealthStatus = null
);
