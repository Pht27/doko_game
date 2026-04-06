using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Application.Games.Queries;

public record PlayerGameView(
    GameId GameId,
    GamePhase Phase,
    PlayerId RequestingPlayer,
    IReadOnlyList<Card> Hand,
    IReadOnlyList<Card> LegalCards,
    IReadOnlyList<AnnouncementType> LegalAnnouncements,
    /// <summary>For each card in hand: the sonderkarten the player could activate by playing it, with display metadata.</summary>
    IReadOnlyDictionary<CardId, IReadOnlyList<SonderkarteInfo>> EligibleSonderkartenPerCard,
    IReadOnlyList<PlayerPublicState> OtherPlayers,
    TrickSummary? CurrentTrick,
    IReadOnlyList<TrickSummary> CompletedTricks,
    PlayerId CurrentTurn,
    bool IsMyTurn
)
{
    /// <summary>
    /// Hand sorted for display: trump highest-to-lowest, then plain suits grouped and sorted.
    /// Computed by <see cref="GameQueryService"/> using the current trump evaluator.
    /// </summary>
    public IReadOnlyList<Card> HandSorted { get; init; } = Hand;

    /// <summary>
    /// Reservation types the player is eligible to declare (during the Reservations phase).
    /// Empty outside the Reservations phase or after the player has already declared.
    /// "Keine Vorbehalt" (null) is always implicitly available and not listed here.
    /// </summary>
    public IReadOnlyList<ReservationPriority> EligibleReservations { get; init; } = [];
}
