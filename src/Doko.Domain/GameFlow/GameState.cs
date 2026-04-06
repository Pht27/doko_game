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
    public PlayerId CurrentTurn { get; private set; }
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
    public IReadOnlyDictionary<PlayerId, Hand>? InitialHands { get; private set; }

    /// <summary>
    /// Round 1 of reservation discovery: each player's health declaration.
    /// True = Vorbehalt (has a reservation), false = Gesund (none).
    /// Null means the player has not yet been asked.
    /// </summary>
    public IReadOnlyDictionary<PlayerId, bool> HealthDeclarations { get; private set; } =
        new Dictionary<PlayerId, bool>();

    /// <summary>
    /// Players still awaiting a declaration in the current reservation check phase
    /// (SoloCheck, ArmutCheck, …). <see cref="CurrentTurn"/> equals the first entry.
    /// Empty outside reservation check phases.
    /// </summary>
    public IReadOnlyList<PlayerId> PendingReservationResponders { get; private set; } = [];

    /// <summary>
    /// Tracks each player's reservation declaration during a check phase.
    /// Populated by <see cref="RecordDeclarationModification"/>; null means the player passed.
    /// Cleared between check phases by <see cref="ClearReservationDeclarationsModification"/>.
    /// </summary>
    public IReadOnlyDictionary<PlayerId, IReservation?> ReservationDeclarations
    {
        get;
        private set;
    } = new Dictionary<PlayerId, IReservation?>();

    /// <summary>The player who declared Armut. Set when Armut wins the reservation check.</summary>
    public PlayerId? ArmutPlayer { get; private set; }

    /// <summary>
    /// The rich player who accepted the Armut. Set during <see cref="GamePhase.ArmutPartnerFinding"/>.
    /// Null if nobody has accepted yet.
    /// </summary>
    public PlayerId? ArmutRichPlayer { get; private set; }

    /// <summary>
    /// How many cards the poor player gave to the rich player in the Armut exchange.
    /// Used to validate the number of cards returned.
    /// </summary>
    public int ArmutTransferCount { get; private set; }

    /// <summary>
    /// Pre-computed trick results (winner + extrapunkt awards) appended at trick completion time.
    /// Parallel to <see cref="CompletedTricks"/>; used by <c>FinishGameUseCase</c> to build
    /// <c>CompletedGame</c> for the scorer.
    /// </summary>
    public IReadOnlyList<TrickResult> ScoredTricks { get; private set; } = [];

    /// <summary>Card-point transfers recorded by Sonderkarten (e.g. Schatz). Used during scoring.</summary>
    public IReadOnlyList<TransferCardPointsModification> CardPointTransfers => _cardPointTransfers;
    private readonly List<TransferCardPointsModification> _cardPointTransfers = new();

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
        PlayerId currentTurn = default,
        ITrumpEvaluator? trumpEvaluator = null,
        IPartyResolver? partyResolver = null,
        PlayDirection direction = PlayDirection.Counterclockwise,
        IReservation? activeReservation = null,
        IReadOnlyList<Trick>? completedTricks = null,
        Trick? currentTrick = null,
        IReadOnlyList<Announcement>? announcements = null,
        IReadOnlyList<SonderkarteType>? activeSonderkarten = null,
        IReadOnlyDictionary<PlayerId, Hand>? initialHands = null,
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
    public PlayerId NextPlayer(PlayerId current, PlayDirection direction)
    {
        var currentPlayer = Players.First(p => p.Id == current);
        int seatIndex = (int)currentPlayer.Seat;
        int nextIndex =
            direction == PlayDirection.Counterclockwise ? (seatIndex + 1) % 4 : (seatIndex + 3) % 4;
        return Players.First(p => (int)p.Seat == nextIndex).Id;
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

            case TransferCardPointsModification m:
                _cardPointTransfers.Add(m);
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

            case SetCurrentTurnModification m:
                CurrentTurn = m.Player;
                break;

            case DealHandsModification m:
                Players = [.. Players.Select(p => p with { Hand = m.Hands[p.Id] })];
                InitialHands = m.Hands;
                break;

            case RecordHealthDeclarationModification m:
                HealthDeclarations = new Dictionary<PlayerId, bool>(HealthDeclarations)
                {
                    [m.Player] = m.HasVorbehalt,
                };
                break;

            case SetPendingRespondersModification m:
                PendingReservationResponders = m.Responders;
                break;

            case ClearReservationDeclarationsModification:
                ReservationDeclarations = new Dictionary<PlayerId, IReservation?>();
                break;

            case SetArmutPlayerModification m:
                ArmutPlayer = m.ArmutPlayer;
                break;

            case SetArmutRichPlayerModification m:
                ArmutRichPlayer = m.RichPlayer;
                break;

            case ArmutGiveTrumpsModification m:
            {
                var poorState = Players.First(p => p.Id == m.PoorPlayer);
                var richState = Players.First(p => p.Id == m.RichPlayer);
                var trumps = poorState
                    .Hand.Cards.Where(c => TrumpEvaluator.IsTrump(c.Type))
                    .ToList();
                ArmutTransferCount = trumps.Count;
                var poorNewHand = new Hands.Hand(poorState.Hand.Cards.Except(trumps).ToList());
                var richNewHand = new Hands.Hand(richState.Hand.Cards.Concat(trumps).ToList());
                Players =
                [
                    .. Players.Select(p =>
                        p.Id == m.PoorPlayer ? p with { Hand = poorNewHand }
                        : p.Id == m.RichPlayer ? p with { Hand = richNewHand }
                        : p
                    ),
                ];
                break;
            }

            case RecordDeclarationModification m:
                var updated = new Dictionary<PlayerId, IReservation?>(ReservationDeclarations)
                {
                    [m.Player] = m.Declaration,
                };
                ReservationDeclarations = updated;
                break;

            case UpdatePlayerHandModification m:
                Players =
                [
                    .. Players.Select(p => p.Id == m.Player ? p with { Hand = m.NewHand } : p),
                ];
                break;

            case SetCurrentTrickModification m:
                CurrentTrick = m.Trick;
                break;

            case AddCompletedTrickModification m:
                CompletedTricks = [.. CompletedTricks, m.Trick];
                ScoredTricks = [.. ScoredTricks, m.Result];
                CurrentTrick = null;
                break;

            case SetPartyResolverModification m:
                PartyResolver = m.Resolver;
                break;

            case AddAnnouncementModification m:
                Announcements = [.. Announcements, m.Announcement];
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
            ActiveReservation?.Apply().TrumpEvaluator ?? NormalTrumpEvaluator.Instance;

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
