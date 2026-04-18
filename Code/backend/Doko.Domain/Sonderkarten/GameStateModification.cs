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
/// If the card IS the trick lead, <see cref="PlayCardHandler"/> applies
/// <see cref="ReverseDirectionModification"/> immediately instead.
/// </summary>
public sealed record ScheduleDirectionFlipModification : GameStateModification;

public sealed record WithdrawAnnouncementModification(PlayerId Player, AnnouncementType Type)
    : GameStateModification;

public sealed record TransferCardPointsModification(CardType From, CardType To)
    : GameStateModification;

public sealed record ActivateSonderkarteModification(SonderkarteType Type) : GameStateModification;

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
public sealed record DealHandsModification(IReadOnlyDictionary<PlayerId, Hand> Hands)
    : GameStateModification;

/// <summary>Records one player's health declaration (Gesund/Vorbehalt) during ReservationHealthCheck.</summary>
public sealed record RecordHealthDeclarationModification(PlayerId Player, bool HasVorbehalt)
    : GameStateModification;

/// <summary>Replaces the list of players still awaiting a response in the current reservation check phase.</summary>
public sealed record SetPendingRespondersModification(IReadOnlyList<PlayerId> Responders)
    : GameStateModification;

/// <summary>Clears all reservation declarations (used when moving between check phases).</summary>
public sealed record ClearReservationDeclarationsModification : GameStateModification;

/// <summary>Sets the player who declared Armut.</summary>
public sealed record SetArmutPlayerModification(PlayerId ArmutPlayer) : GameStateModification;

/// <summary>Sets the rich player who accepted the Armut.</summary>
public sealed record SetArmutRichPlayerModification(PlayerId RichPlayer) : GameStateModification;

/// <summary>
/// Transfers all trump cards from the poor player's hand to the rich player's hand and
/// records the transfer count in <c>ArmutTransferCount</c>.
/// </summary>
public sealed record ArmutGiveTrumpsModification(PlayerId PoorPlayer, PlayerId RichPlayer)
    : GameStateModification;

/// <summary>Records one player's reservation declaration during the reservation phase.</summary>
public sealed record RecordDeclarationModification(PlayerId Player, IReservation? Declaration)
    : GameStateModification;

/// <summary>Replaces a player's hand with a new hand (e.g. after playing a card).</summary>
public sealed record UpdatePlayerHandModification(PlayerId Player, Hand NewHand)
    : GameStateModification;

/// <summary>Sets the current trick (null clears the current trick after it completes).</summary>
public sealed record SetCurrentTrickModification(Tricks.Trick? Trick) : GameStateModification;

/// <summary>Adds a card played by a player to the current trick.</summary>
public sealed record AddCardToTrickModification(Players.PlayerId Player, Cards.Card Card)
    : GameStateModification;

/// <summary>
/// Appends a completed trick to <c>CompletedTricks</c> and its scored result to <c>ScoredTricks</c>,
/// then clears the current trick.
/// </summary>
public sealed record AddCompletedTrickModification(Tricks.Trick Trick, Scoring.TrickResult Result)
    : GameStateModification;

/// <summary>
/// Records that a Genscher (Genscherdamen or Gegengenscherdamen) was activated and the
/// playing player has chosen a partner. GameState.Apply creates the GenscherPartyResolver
/// internally, so the Application layer does not need to know about it.
/// </summary>
public sealed record SetGenscherPartnerModification(
    Players.PlayerId Genscher,
    Players.PlayerId Partner
) : GameStateModification;

/// <summary>Appends an announcement to the game state.</summary>
public sealed record AddAnnouncementModification(Announcements.Announcement Announcement)
    : GameStateModification;

/// <summary>
/// Marks the activation window for a sonderkarte as permanently closed.
/// Applied when a player plays the triggering card but does not activate an eligible sonderkarte
/// whose <c>WindowClosesWhenDeclined</c> is true.
/// </summary>
public sealed record CloseActivationWindowModification(SonderkarteType Type)
    : GameStateModification;

/// <summary>
/// Records whether the rich player's returned cards included any trump during
/// <see cref="GamePhase.ArmutCardExchange"/>. Used to display the exchange announcement.
/// </summary>
public sealed record SetArmutReturnedTrumpModification(bool IncludedTrump) : GameStateModification;

/// <summary>
/// Records the VorbehaltRauskommer — the player who leads the reservation-check ordering
/// for this round. Set once at deal time; used by MakeReservationHandler to pick
/// who plays the first card in Normal/Hochzeit/SchlankerMartin games.
/// </summary>
public sealed record SetVorbehaltRauskommerModification(PlayerId Player) : GameStateModification;
