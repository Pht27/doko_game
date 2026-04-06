using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Hands;
using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Domain.Sonderkarten;

public abstract record GameStateModification;

public sealed record ReverseDirectionModification : GameStateModification;

/// <summary>
/// Schedules a direction reversal to take effect at the start of the next trick.
/// Used when LinksGehangter/RechtsGehangter fires on a non-lead card mid-trick.
/// If the card IS the trick lead, <see cref="PlayCardUseCase"/> applies
/// <see cref="ReverseDirectionModification"/> immediately instead.
/// </summary>
public sealed record ScheduleDirectionFlipModification : GameStateModification;

public sealed record WithdrawAnnouncementModification(
    PlayerId Player,
    AnnouncementType Type) : GameStateModification;

public sealed record TransferCardPointsModification(
    CardType From,
    CardType To) : GameStateModification;

public sealed record ActivateSonderkarteModification(
    SonderkarteType Type) : GameStateModification;

/// <summary>
/// Signals that the trump evaluator must be rebuilt.
/// Returned from <see cref="ISonderkarte"/> by sonderkarten that affect trump order
/// (e.g. Schweinchen, Heidmann, Heidfrau). The rebuild reads ranking modifiers and
/// suppressions directly from <see cref="GameState.ActiveSonderkarten"/>.
/// Sonderkarten without trump effects (Kemmerich, Genscherdamen, …) do not return this.
/// </summary>
public sealed record RebuildTrumpEvaluatorModification : GameStateModification;

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

/// <summary>
/// Replaces the active party resolver without touching the trump evaluator.
/// Used when Genscherdamen/Gegengenscherdamen fires and the Genscher picks a new partner.
/// </summary>
public sealed record SetPartyResolverModification(Parties.IPartyResolver Resolver) : GameStateModification;

/// <summary>Appends an announcement to the game state.</summary>
public sealed record AddAnnouncementModification(
    Announcements.Announcement Announcement) : GameStateModification;

/// <summary>
/// Marks the activation window for a sonderkarte as permanently closed.
/// Applied when a player plays the triggering card but does not activate an eligible sonderkarte
/// whose <c>WindowClosesWhenDeclined</c> is true.
/// </summary>
public sealed record CloseActivationWindowModification(
    SonderkarteType Type) : GameStateModification;
