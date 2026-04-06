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
    /// True when it is this player's turn to declare their health status
    /// (Gesund or Vorbehalt) in <see cref="GamePhase.ReservationHealthCheck"/>.
    /// </summary>
    public bool ShouldDeclareHealth { get; init; } = false;

    /// <summary>
    /// Reservation types the player is eligible to declare in the current check phase
    /// (SoloCheck, ArmutCheck, SchmeissenCheck, HochzeitCheck).
    /// Empty outside those phases or when it is not this player's turn.
    /// "Keine Vorbehalt" (pass) is always implicitly available and not listed here.
    /// </summary>
    public IReadOnlyList<ReservationPriority> EligibleReservations { get; init; } = [];

    /// <summary>
    /// True when it is this player's turn to respond to an Armut partner request
    /// in <see cref="GamePhase.ArmutPartnerFinding"/>.
    /// </summary>
    public bool ShouldRespondToArmut { get; init; } = false;

    /// <summary>
    /// True when this player is the rich player and must return cards during
    /// <see cref="GamePhase.ArmutCardExchange"/>.
    /// </summary>
    public bool ShouldReturnArmutCards { get; init; } = false;

    /// <summary>How many cards the rich player must return. Null outside <see cref="GamePhase.ArmutCardExchange"/>.</summary>
    public int? ArmutCardReturnCount { get; init; } = null;
}
