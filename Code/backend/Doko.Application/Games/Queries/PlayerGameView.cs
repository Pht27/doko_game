using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Application.Games.Queries;

public record PlayerGameView(
    GameId GameId,
    GamePhase Phase,
    PlayerSeat RequestingPlayer,
    Party? OwnParty,
    IReadOnlyList<Card> Hand,
    IReadOnlyList<Card> LegalCards,
    IReadOnlyList<AnnouncementType> LegalAnnouncements,
    /// <summary>For each card in hand: the sonderkarten the player could activate by playing it, with display metadata.</summary>
    IReadOnlyDictionary<CardId, IReadOnlyList<SonderkarteInfo>> EligibleSonderkartenPerCard,
    IReadOnlyList<PlayerPublicState> OtherPlayers,
    TrickSummary? CurrentTrick,
    IReadOnlyList<TrickSummary> CompletedTricks,
    PlayerSeat CurrentTurn,
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
    /// "Keine Vorbehalt" (pass) is implicitly available unless <see cref="MustDeclareReservation"/> is true.
    /// </summary>
    public IReadOnlyList<ReservationPriority> EligibleReservations { get; init; } = [];

    /// <summary>
    /// True when it is this player's turn to declare in any reservation check phase
    /// (SoloCheck, ArmutCheck, SchmeissenCheck, HochzeitCheck), even if no specific reservation
    /// is eligible (in which case the player must pass).
    /// </summary>
    public bool ShouldDeclareReservation { get; init; } = false;

    /// <summary>
    /// True when the player is not permitted to pass — they are the sole Vorbehalt player and
    /// must declare a specific reservation.
    /// </summary>
    public bool MustDeclareReservation { get; init; } = false;

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

    /// <summary>
    /// Number of cards exchanged in the Armut. Non-null after the exchange completes (i.e. during
    /// the Playing phase of an Armut game). Used to display the exchange announcement to all players.
    /// </summary>
    public int? ArmutExchangeCardCount { get; init; } = null;

    /// <summary>
    /// Whether the rich player's returned cards included any trump. Non-null after the exchange.
    /// </summary>
    public bool? ArmutReturnedTrump { get; init; } = null;

    /// <summary>
    /// The active game mode, derived from the winning reservation's priority
    /// (e.g. "KaroSolo", "Hochzeit", "Armut"). Null means Normalspiel.
    /// </summary>
    public string? ActiveGameMode { get; init; } = null;

    /// <summary>
    /// True when this player won the second ♠Q trick and must choose a solo in
    /// <see cref="GamePhase.SchwarzesSauSoloSelect"/>.
    /// </summary>
    public bool ShouldChooseSchwarzesSauSolo { get; init; } = false;

    /// <summary>
    /// The solos the player may choose from during <see cref="GamePhase.SchwarzesSauSoloSelect"/>.
    /// All priorities KaroSolo (0) through SchlankerMartin (8). Empty outside that phase or
    /// when it is not this player's turn.
    /// </summary>
    public IReadOnlyList<ReservationPriority> EligibleSchwarzesSauSolos { get; init; } = [];

    /// <summary>
    /// The display label for the requesting player's highest effective announcement,
    /// or null if they have not announced. Matches the format used for other players:
    /// "Re" / "Kontra" for the Win announcement, "Keine90" etc. for higher ones.
    /// </summary>
    public string? OwnHighestAnnouncement { get; init; } = null;

    /// <summary>
    /// The seat number of the player who declared the active game mode (Solo, Hochzeit, Armut).
    /// Null for Normalspiel or when no explicit declarant exists.
    /// </summary>
    public int? GameModePlayerSeat { get; init; } = null;
}
