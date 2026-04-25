using Doko.Domain.Announcements;
using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Rules;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;
using Doko.Domain.Trump;

namespace Doko.Domain.GameFlow;

public sealed class GameState
{
    public GameId Id { get; private set; }
    public GamePhase Phase { get; private set; }

    /// <summary>Immutable configuration set once at game creation.</summary>
    public RuleSet Rules { get; private set; }

    public IReadOnlyList<PlayerState> Players { get; private set; }
    public PlayerSeat CurrentTurn { get; private set; }
    public PlayDirection Direction { get; private set; }

    public IReservation? ActiveReservation { get; private set; }
    public IReadOnlyList<Trick> CompletedTricks { get; private set; }
    public Trick? CurrentTrick { get; private set; }

    public IReadOnlyList<Announcement> Announcements { get; private set; }
    public IReadOnlyList<SonderkarteType> ActiveSonderkarten { get; private set; }

    /// <summary>
    /// Sonderkarten whose activation window has permanently closed: the triggering card was played
    /// but the player chose not to activate. Checked by <see cref="ISonderkarte.AreConditionsMet"/>.
    /// </summary>
    public IReadOnlySet<SonderkarteType> ClosedWindows { get; private set; } =
        new HashSet<SonderkarteType>();

    public ITrumpEvaluator TrumpEvaluator { get; private set; }
    public IPartyResolver PartyResolver { get; private set; }

    /// <summary>
    /// True when a direction reversal (LinksGehangter/RechtsGehangter) was activated mid-trick
    /// and should take effect at the start of the next trick.
    /// Cleared automatically when <see cref="ReverseDirectionModification"/> is applied.
    /// </summary>
    public bool DirectionFlipPending { get; private set; }

    /// <summary>
    /// Each player's hand as originally dealt. Set once when dealing completes, never mutated.
    /// Used by sonderkarte eligibility checks that need to know what a player originally held
    /// (e.g. Superschweinchen: player originally held both ♦10, but may have already played the first).
    /// Null before dealing phase completes.
    /// </summary>
    public IReadOnlyDictionary<PlayerSeat, Hand>? InitialHands { get; private set; }

    /// <summary>
    /// Round 1 of reservation discovery: each player's health declaration.
    /// True = Vorbehalt (has a reservation), false = Gesund (none).
    /// Null means the player has not yet been asked.
    /// </summary>
    public IReadOnlyDictionary<PlayerSeat, bool> HealthDeclarations { get; private set; } =
        new Dictionary<PlayerSeat, bool>();

    /// <summary>
    /// Players still awaiting a declaration in the current reservation check phase
    /// (SoloCheck, ArmutCheck, …). <see cref="CurrentTurn"/> equals the first entry.
    /// Empty outside reservation check phases.
    /// </summary>
    public IReadOnlyList<PlayerSeat> PendingReservationResponders { get; private set; } = [];

    /// <summary>
    /// Tracks each player's reservation declaration during a check phase.
    /// Populated by <see cref="RecordDeclarationModification"/>; null means the player passed.
    /// Cleared between check phases by <see cref="ClearReservationDeclarationsModification"/>.
    /// </summary>
    public IReadOnlyDictionary<PlayerSeat, IReservation?> ReservationDeclarations
    {
        get;
        private set;
    } = new Dictionary<PlayerSeat, IReservation?>();

    /// <summary>The player who declared Armut. Set when Armut wins the reservation check.</summary>
    public PlayerSeat? ArmutPlayer { get; private set; }

    /// <summary>
    /// The rich player who accepted the Armut. Set during <see cref="GamePhase.ArmutPartnerFinding"/>.
    /// Null if nobody has accepted yet.
    /// </summary>
    public PlayerSeat? ArmutRichPlayer { get; private set; }

    /// <summary>
    /// How many cards the poor player gave to the rich player in the Armut exchange.
    /// Used to validate the number of cards returned.
    /// </summary>
    public int ArmutTransferCount { get; private set; }

    /// <summary>
    /// Whether the cards returned by the rich player during <see cref="GamePhase.ArmutCardExchange"/>
    /// included any trump. Null before the exchange completes.
    /// </summary>
    public bool? ArmutReturnedTrump { get; private set; }

    /// <summary>
    /// Pre-computed trick results (winner + extrapunkt awards) appended at trick completion time.
    /// Parallel to <see cref="CompletedTricks"/>; used by <c>FinishGameHandler</c> to build
    /// <c>CompletedGame</c> for the scorer.
    /// </summary>
    public IReadOnlyList<TrickResult> ScoredTricks { get; private set; } = [];

    /// <summary>
    /// True once a Genscher (or Gegengenscher) changed the actual team composition.
    /// Set to false again only if Gegengenscherdamen restores the exact original Re pair.
    /// When true, Feigheit does not apply at scoring time.
    /// </summary>
    public bool GenscherTeamsChanged { get; private set; }

    /// <summary>
    /// The two Re players before the first team-changing Genscher fired.
    /// Used by Gegengenscherdamen to detect whether the original teams are restored.
    /// Null until a team-changing Genscher fires; cleared on full restoration.
    /// </summary>
    public (PlayerSeat First, PlayerSeat Second)? PreGenscherRePlayers { get; private set; }

    /// <summary>
    /// Announcements saved when a team-changing Genscherdamen fired and cleared <see cref="Announcements"/>.
    /// Restored if Gegengenscherdamen subsequently recreates the original teams.
    /// Null when no saved announcements exist.
    /// </summary>
    public IReadOnlyList<Announcement>? SavedGenscherAnnouncements { get; private set; }

    /// <summary>
    /// The player who leads the reservation-check ordering for this round.
    /// Rotates counter-clockwise each game. Always rotates; SpieleRauskommer
    /// (who actually plays first) may differ for Solo/Armut.
    /// </summary>
    public PlayerSeat VorbehaltRauskommer { get; private set; }

    /// <summary>
    /// Active silent (undeclared) game mode, set when all players declare Gesund and a player's
    /// hand qualifies for Kontrasolo or Stille Hochzeit. Null in all other game modes.
    /// </summary>
    public SilentGameMode? SilentMode { get; private set; }

    /// <summary>
    /// True once a Hochzeit failed to find a partner in 3 qualifying tricks and became a
    /// forced solo. Affects scoring (soloFactor=3), Feigheit exemption, and Rauskommer advance.
    /// </summary>
    public bool HochzeitBecameForcedSolo { get; private set; }

    /// <summary>
    /// True when the game is running as Schwarze Sau (Armut with no partner found).
    /// The game watches for the second ♠Q trick and then interrupts with
    /// <see cref="GamePhase.SchwarzesSauSoloSelect"/>.
    /// </summary>
    public bool IsSchwarzesSau { get; private set; }

#pragma warning disable CS8618 // Non-nullable fields initialized via factory/Apply
    private GameState() { }
#pragma warning restore CS8618

    /// <summary>
    /// Creates a <see cref="GameState"/> with the given configuration. All parameters are optional
    /// and fall back to sensible defaults so tests only need to supply what they care about.
    /// </summary>
    public static GameState Create(
        RuleSet? rules = null,
        IReadOnlyList<PlayerState>? players = null,
        PlayerSeat currentTurn = default,
        ITrumpEvaluator? trumpEvaluator = null,
        IPartyResolver? partyResolver = null,
        PlayDirection direction = PlayDirection.Counterclockwise,
        IReservation? activeReservation = null,
        IReadOnlyList<Trick>? completedTricks = null,
        Trick? currentTrick = null,
        IReadOnlyList<Announcement>? announcements = null,
        IReadOnlyList<SonderkarteType>? activeSonderkarten = null,
        IReadOnlyDictionary<PlayerSeat, Hand>? initialHands = null,
        GamePhase phase = GamePhase.Dealing,
        GameId id = default
    ) =>
        new GameState
        {
            Id = id.Value == Guid.Empty ? GameId.New() : id,
            Phase = phase,
            Rules = rules ?? RuleSet.Default(),
            Players = players ?? [],
            CurrentTurn = currentTurn,
            Direction = direction,
            ActiveReservation = activeReservation,
            CompletedTricks = completedTricks ?? [],
            CurrentTrick = currentTrick,
            Announcements = announcements ?? [],
            ActiveSonderkarten = activeSonderkarten ?? [],
            TrumpEvaluator = trumpEvaluator ?? NormalTrumpEvaluator.Instance,
            PartyResolver = partyResolver ?? NormalPartyResolver.Instance,
            InitialHands = initialHands,
        };

    /// <summary>Determines the next player in the given play direction.</summary>
    public PlayerSeat NextPlayer(PlayerSeat current, PlayDirection direction)
    {
        int seatIndex = (int)current;
        int nextIndex =
            direction == PlayDirection.Counterclockwise ? (seatIndex + 1) % 4 : (seatIndex + 3) % 4;
        return (PlayerSeat)nextIndex;
    }

    /// <summary>Applies a state modification. The only place mutations occur.</summary>
    public void Apply(GameStateModification modification)
    {
        switch (modification)
        {
            case ReverseDirectionModification:
                Direction =
                    Direction == PlayDirection.Counterclockwise
                        ? PlayDirection.Clockwise
                        : PlayDirection.Counterclockwise;
                DirectionFlipPending = false;
                break;

            case ScheduleDirectionFlipModification:
                DirectionFlipPending = true;
                break;

            case WithdrawAnnouncementModification m:
                Announcements = Announcements
                    .Where(a => !(a.Player == m.Player && a.Type == m.Type))
                    .ToList();
                break;

            case ActivateSonderkarteModification m:
                ActiveSonderkarten = ActiveSonderkarten.Append(m.Type).ToList();
                break;

            case RebuildTrumpEvaluatorModification:
                RebuildTrumpEvaluator();
                break;

            case CloseActivationWindowModification m:
                ClosedWindows = new HashSet<SonderkarteType>(ClosedWindows) { m.Type };
                break;

            case AdvancePhaseModification m:
                Phase = m.NewPhase;
                break;

            case SetGameModeModification m:
                ActiveReservation = m.Reservation;
                var ctx = m.Reservation?.Apply();
                TrumpEvaluator = ctx?.TrumpEvaluator ?? NormalTrumpEvaluator.Instance;
                PartyResolver = ctx?.PartyResolver ?? NormalPartyResolver.Instance;
                break;

            case SetSilentGameModeModification m:
                SilentMode = m.Mode;
                switch (m.Mode?.Type)
                {
                    case SilentGameModeType.KontraSolo:
                        TrumpEvaluator = KontraSoloTrumpEvaluator.Instance;
                        PartyResolver = new KontraSoloPartyResolver(m.Mode.Player);
                        break;
                    case SilentGameModeType.StilleHochzeit:
                        TrumpEvaluator = NormalTrumpEvaluator.Instance;
                        PartyResolver = new StilleHochzeitPartyResolver(m.Mode.Player);
                        break;
                    default:
                        TrumpEvaluator = NormalTrumpEvaluator.Instance;
                        PartyResolver = NormalPartyResolver.Instance;
                        break;
                }
                break;

            case SetCurrentTurnModification m:
                CurrentTurn = m.Player;
                break;

            case DealHandsModification m:
                Players = [.. Players.Select(p => p with { Hand = m.Hands[p.Seat] })];
                InitialHands = m.Hands;
                break;

            case RecordHealthDeclarationModification m:
                HealthDeclarations = new Dictionary<PlayerSeat, bool>(HealthDeclarations)
                {
                    [m.Player] = m.HasVorbehalt,
                };
                break;

            case SetPendingRespondersModification m:
                PendingReservationResponders = m.Responders;
                break;

            case ClearReservationDeclarationsModification:
                ReservationDeclarations = new Dictionary<PlayerSeat, IReservation?>();
                break;

            case SetArmutPlayerModification m:
                ArmutPlayer = m.ArmutPlayer;
                break;

            case SetArmutRichPlayerModification m:
                ArmutRichPlayer = m.RichPlayer;
                break;

            case ArmutGiveTrumpsModification m:
            {
                var poorState = Players.First(p => p.Seat == m.PoorPlayer);
                var richState = Players.First(p => p.Seat == m.RichPlayer);
                var trumps = poorState
                    .Hand.Cards.Where(c => TrumpEvaluator.IsTrump(c.Type))
                    .ToList();
                ArmutTransferCount = trumps.Count;
                var poorNewHand = new Hands.Hand(poorState.Hand.Cards.Except(trumps).ToList());
                var richNewHand = new Hands.Hand(richState.Hand.Cards.Concat(trumps).ToList());
                Players =
                [
                    .. Players.Select(p =>
                        p.Seat == m.PoorPlayer ? p with { Hand = poorNewHand }
                        : p.Seat == m.RichPlayer ? p with { Hand = richNewHand }
                        : p
                    ),
                ];
                break;
            }

            case SetArmutReturnedTrumpModification m:
                ArmutReturnedTrump = m.IncludedTrump;
                break;

            case RecordDeclarationModification m:
                var updated = new Dictionary<PlayerSeat, IReservation?>(ReservationDeclarations)
                {
                    [m.Player] = m.Declaration,
                };
                ReservationDeclarations = updated;
                break;

            case UpdatePlayerHandModification m:
                Players =
                [
                    .. Players.Select(p => p.Seat == m.Player ? p with { Hand = m.NewHand } : p),
                ];
                break;

            case SetCurrentTrickModification m:
                CurrentTrick = m.Trick;
                break;

            case AddCardToTrickModification m:
                CurrentTrick!.Add(new Tricks.TrickCard(m.Card, m.Player));
                break;

            case AddCompletedTrickModification m:
                CompletedTricks = [.. CompletedTricks, m.Trick];
                ScoredTricks = [.. ScoredTricks, m.Result];
                CurrentTrick = null;
                break;

            case SetGenscherPartnerModification m:
            {
                // In silent solos (StilleHochzeit, KontraSolo), Genscher can be announced but
                // parties are fixed by the solo logic — all side effects are suppressed.
                if (SilentMode is not null)
                    break;

                bool teamsChanged =
                    PartyResolver.ResolveParty(m.Genscher, this)
                    != PartyResolver.ResolveParty(m.Partner, this);

                if (teamsChanged)
                {
                    if (PreGenscherRePlayers is null)
                    {
                        // First team-changing Genscher: save original Re pair and announcements.
                        var rePlayers = Players
                            .Where(p => PartyResolver.ResolveParty(p.Seat, this) == Party.Re)
                            .Select(p => p.Seat)
                            .ToArray();
                        PreGenscherRePlayers = (rePlayers[0], rePlayers[1]);
                        SavedGenscherAnnouncements = Announcements;
                        Announcements = [];
                        GenscherTeamsChanged = true;
                    }
                    else
                    {
                        // Subsequent Genscher (Gegengenscher): check for original team restoration.
                        var (orig1, orig2) = PreGenscherRePlayers.Value;
                        bool restored =
                            (m.Genscher == orig1 || m.Genscher == orig2)
                            && (m.Partner == orig1 || m.Partner == orig2);
                        if (restored && SavedGenscherAnnouncements is not null)
                        {
                            Announcements = SavedGenscherAnnouncements;
                            SavedGenscherAnnouncements = null;
                            PreGenscherRePlayers = null;
                            GenscherTeamsChanged = false;
                        }
                        else
                        {
                            // Different Gegengenscher outcome — saved announcements are lost.
                            SavedGenscherAnnouncements = null;
                        }
                    }
                }
                // If !teamsChanged (Nicht tauschen): announcements stay as-is.

                PartyResolver = new Parties.GenscherPartyResolver(m.Genscher, m.Partner);
                break;
            }

            case AddAnnouncementModification m:
                Announcements = [.. Announcements, m.Announcement];
                break;

            case SetVorbehaltRauskommerModification m:
                VorbehaltRauskommer = m.Player;
                break;

            case SetHochzeitForcedSoloModification:
                HochzeitBecameForcedSolo = true;
                break;

            case SetSchwarzesSauModification:
                IsSchwarzesSau = true;
                break;

            case ClearActiveSonderkartenModification:
                ActiveSonderkarten = [];
                ClosedWindows = new HashSet<SonderkarteType>();
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(modification),
                    $"Unknown modification type: {modification.GetType().Name}"
                );
        }
    }

    private void RebuildTrumpEvaluator()
    {
        var baseEvaluator =
            ActiveReservation?.Apply().TrumpEvaluator
            ?? (
                SilentMode?.Type == SilentGameModeType.KontraSolo
                    ? (ITrumpEvaluator)KontraSoloTrumpEvaluator.Instance
                    : NormalTrumpEvaluator.Instance
            );

        var activeSet = ActiveSonderkarten.ToHashSet();
        var suppressed = SonderkarteRegistry
            .GetEnabled(Rules)
            .Where(s => activeSet.Contains(s.Type) && s.Suppresses.HasValue)
            .Select(s => s.Suppresses!.Value)
            .ToHashSet();

        var modifiers = SonderkarteRegistry
            .GetEnabled(Rules)
            .Where(s =>
                activeSet.Contains(s.Type)
                && !suppressed.Contains(s.Type)
                && s.RankingModifier is not null
            )
            .Select(s => s.RankingModifier!)
            .ToList();

        TrumpEvaluator =
            modifiers.Count > 0
                ? new StandardSonderkarteDecorator(baseEvaluator, modifiers)
                : baseEvaluator;
    }
}
