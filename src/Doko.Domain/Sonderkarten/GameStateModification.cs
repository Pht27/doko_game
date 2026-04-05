using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Hands;
using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Domain.Sonderkarten;

public abstract record GameStateModification;

public sealed record ReverseDirectionModification : GameStateModification;

public sealed record WithdrawAnnouncementModification(
    PlayerId Player,
    AnnouncementType Type) : GameStateModification;

public sealed record TransferCardPointsModification(
    CardType From,
    CardType To) : GameStateModification;

public sealed record ActivateSonderkarteModification(
    SonderkarteType Type) : GameStateModification;

/// <summary>Advances the game to a new phase.</summary>
public sealed record AdvancePhaseModification(GamePhase NewPhase) : GameStateModification;

/// <summary>
/// Resolves the game mode after reservations: sets the active reservation (null = normal game),
/// and rebuilds the trump evaluator and party resolver accordingly.
/// </summary>
public sealed record SetGameModeModification(IReservation? Reservation) : GameStateModification;

/// <summary>Sets whose turn it is.</summary>
public sealed record SetCurrentTurnModification(PlayerId Player) : GameStateModification;

/// <summary>Deals hands to all players and records the initial hand snapshot.</summary>
public sealed record DealHandsModification(
    IReadOnlyDictionary<PlayerId, Hand> Hands) : GameStateModification;

/// <summary>Records one player's reservation declaration during the reservation phase.</summary>
public sealed record RecordDeclarationModification(
    PlayerId Player,
    IReservation? Declaration) : GameStateModification;

/// <summary>Replaces a player's hand with a new hand (e.g. after playing a card).</summary>
public sealed record UpdatePlayerHandModification(
    PlayerId Player,
    Hand NewHand) : GameStateModification;

/// <summary>Sets the current trick (null clears the current trick after it completes).</summary>
public sealed record SetCurrentTrickModification(Tricks.Trick? Trick) : GameStateModification;

/// <summary>
/// Appends a completed trick to <c>CompletedTricks</c> and its scored result to <c>ScoredTricks</c>,
/// then clears the current trick.
/// </summary>
public sealed record AddCompletedTrickModification(
    Tricks.Trick Trick,
    Scoring.TrickResult Result) : GameStateModification;

/// <summary>Appends an announcement to the game state.</summary>
public sealed record AddAnnouncementModification(
    Announcements.Announcement Announcement) : GameStateModification;
