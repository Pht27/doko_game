using Doko.Domain.Announcements;
using Doko.Domain.GameFlow.Modifications;
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

public abstract record GameState
{
    public GameId Id { get; init; }
    public GamePhase Phase { get; init; }

    /// <summary>Immutable configuration set once at game creation.</summary>
    public RuleSet Rules { get; init; } = null!;

    public IReadOnlyList<PlayerState> Players { get; init; } = [];
    public PlayerSeat CurrentTurn { get; init; }
    public PlayDirection Direction { get; init; }

    public IReservation? ActiveReservation { get; init; }
    public IReadOnlyList<Trick> CompletedTricks { get; init; } = [];
    public Trick? CurrentTrick { get; init; }

    public IReadOnlyList<Announcement> Announcements { get; init; } = [];
    public IReadOnlyList<SonderkarteType> ActiveSonderkarten { get; init; } = [];

    /// <summary>
    /// Sonderkarten whose activation window has permanently closed: the triggering card was played
    /// but the player chose not to activate. Checked by <see cref="ISonderkarte.AreConditionsMet"/>.
    /// </summary>
    public IReadOnlySet<SonderkarteType> ClosedWindows { get; init; } =
        new HashSet<SonderkarteType>();

    public ITrumpEvaluator TrumpEvaluator { get; init; } = null!;
    public IPartyResolver PartyResolver { get; init; } = null!;

    /// <summary>
    /// True when a direction reversal (LinksGehangter/RechtsGehangter) was activated mid-trick
    /// and should take effect at the start of the next trick.
    /// Cleared automatically when <see cref="ReverseDirectionModification"/> is applied.
    /// </summary>
    public bool DirectionFlipPending { get; init; }

    /// <summary>
    /// Each player's hand as originally dealt. Set once when dealing completes, never mutated.
    /// Used by sonderkarte eligibility checks that need to know what a player originally held
    /// (e.g. Superschweinchen: player originally held both ♦10, but may have already played the first).
    /// Null before dealing phase completes.
    /// </summary>
    public IReadOnlyDictionary<PlayerSeat, Hand>? InitialHands { get; init; }

    /// <summary>
    /// Round 1 of reservation discovery: each player's health declaration.
    /// True = Vorbehalt (has a reservation), false = Gesund (none).
    /// Null means the player has not yet been asked.
    /// </summary>
    public IReadOnlyDictionary<PlayerSeat, bool> HealthDeclarations { get; init; } =
        new Dictionary<PlayerSeat, bool>();

    /// <summary>
    /// Players still awaiting a declaration in the current reservation check phase
    /// (SoloCheck, ArmutCheck, …). <see cref="CurrentTurn"/> equals the first entry.
    /// Empty outside reservation check phases.
    /// </summary>
    public IReadOnlyList<PlayerSeat> PendingReservationResponders { get; init; } = [];

    /// <summary>
    /// Tracks each player's reservation declaration during a check phase.
    /// Populated by <see cref="RecordDeclarationModification"/>; null means the player passed.
    /// Cleared between check phases by <see cref="ClearReservationDeclarationsModification"/>.
    /// </summary>
    public IReadOnlyDictionary<PlayerSeat, IReservation?> ReservationDeclarations { get; init; } =
        new Dictionary<PlayerSeat, IReservation?>();

    /// <summary>The player who declared the active game mode (Solo, Hochzeit, Armut player). Null for Normalspiel.</summary>
    public PlayerSeat? GameModePlayerSeat { get; init; }

    /// <summary>
    /// Armut-phase state. Non-null iff an Armut game mode is active.
    /// Initialized when the Armut player is set; updated through the card-exchange phase.
    /// </summary>
    public ArmutState? Armut { get; init; }

    /// <summary>
    /// Pre-computed trick results (winner + extrapunkt awards) appended at trick completion time.
    /// Parallel to <see cref="CompletedTricks"/>; used by <c>FinishGameHandler</c> to build
    /// <c>CompletedGame</c> for the scorer.
    /// </summary>
    public IReadOnlyList<TrickResult> ScoredTricks { get; init; } = [];

    /// <summary>
    /// Genscher-phase state. Non-null once a team-changing Genscher has fired.
    /// Null again only if Gegengenscherdamen fully restores the original Re pair.
    /// When non-null and <see cref="GenscherState.TeamsChanged"/> is true, Feigheit does not apply.
    /// </summary>
    public GenscherState? Genscher { get; init; }

    /// <summary>
    /// The player who leads the reservation-check ordering for this round.
    /// Rotates counter-clockwise each game. Always rotates; SpieleRauskommer
    /// (who actually plays first) may differ for Solo/Armut.
    /// </summary>
    public PlayerSeat VorbehaltRauskommer { get; init; }

    /// <summary>
    /// Active silent (undeclared) game mode, set when all players declare Gesund and a player's
    /// hand qualifies for Kontrasolo or Stille Hochzeit. Null in all other game modes.
    /// </summary>
    public SilentGameMode? SilentMode { get; init; }

    /// <summary>
    /// True once a Hochzeit failed to find a partner in 3 qualifying tricks and became a
    /// forced solo. Affects scoring (soloFactor=3), Feigheit exemption, and Rauskommer advance.
    /// </summary>
    public bool HochzeitBecameForcedSolo { get; init; }

    /// <summary>
    /// True when the game is running as Schwarze Sau (Armut with no partner found).
    /// The game watches for the second ♠Q trick and then interrupts with
    /// <see cref="GamePhase.SchwarzesSauSoloSelect"/>.
    /// </summary>
    public bool IsSchwarzesSau { get; init; }

    private ITrumpEvaluatorFactory Factory { get; init; } = TrumpEvaluatorFactory.Instance;

    /// <summary>
    /// Creates a <see cref="GameState"/> with the given configuration. All parameters are optional
    /// and fall back to sensible defaults so tests only need to supply what they care about.
    /// Returns the concrete subtype that corresponds to <paramref name="phase"/>.
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
        IReadOnlyList<TrickResult>? scoredTricks = null,
        Trick? currentTrick = null,
        IReadOnlyList<Announcement>? announcements = null,
        IReadOnlyList<SonderkarteType>? activeSonderkarten = null,
        IReadOnlyDictionary<PlayerSeat, Hand>? initialHands = null,
        GamePhase phase = GamePhase.Dealing,
        GameId id = default,
        ITrumpEvaluatorFactory? factory = null
    )
    {
        var resolvedRules = rules ?? RuleSet.Default();
        var resolvedEvaluator = trumpEvaluator ?? NormalTrumpEvaluator.Instance;
        var resolvedTricks = completedTricks ?? [];
        var resolvedScored =
            scoredTricks
            ?? resolvedTricks
                .Select(t => new TrickResult(
                    t,
                    t.Winner(resolvedEvaluator, resolvedRules.DulleRule),
                    []
                ))
                .ToList();
        var resolvedFactory = factory ?? TrumpEvaluatorFactory.Instance;
        var resolvedId = id.Value == Guid.Empty ? GameId.New() : id;

        var common = new
        {
            Id = resolvedId,
            Phase = phase,
            Rules = resolvedRules,
            Players = players ?? (IReadOnlyList<PlayerState>)[],
            CurrentTurn = currentTurn,
            Direction = direction,
            ActiveReservation = activeReservation,
            CompletedTricks = resolvedTricks,
            ScoredTricks = resolvedScored,
            CurrentTrick = currentTrick,
            Announcements = announcements ?? (IReadOnlyList<Announcement>)[],
            ActiveSonderkarten = activeSonderkarten ?? (IReadOnlyList<SonderkarteType>)[],
            TrumpEvaluator = resolvedEvaluator,
            PartyResolver = partyResolver ?? NormalPartyResolver.Instance,
            InitialHands = initialHands,
            Factory = resolvedFactory,
        };

        return phase switch
        {
            GamePhase.Dealing => new DealingState
            {
                Id = common.Id,
                Phase = common.Phase,
                Rules = common.Rules,
                Players = common.Players,
                CurrentTurn = common.CurrentTurn,
                Direction = common.Direction,
                ActiveReservation = common.ActiveReservation,
                CompletedTricks = common.CompletedTricks,
                ScoredTricks = common.ScoredTricks,
                CurrentTrick = common.CurrentTrick,
                Announcements = common.Announcements,
                ActiveSonderkarten = common.ActiveSonderkarten,
                TrumpEvaluator = common.TrumpEvaluator,
                PartyResolver = common.PartyResolver,
                InitialHands = common.InitialHands,
                Factory = common.Factory,
            },
            GamePhase.ReservationHealthCheck
            or GamePhase.ReservationSoloCheck
            or GamePhase.ReservationArmutCheck
            or GamePhase.ReservationSchmeissenCheck
            or GamePhase.ReservationHochzeitCheck => new ReservationState
            {
                Id = common.Id,
                Phase = common.Phase,
                Rules = common.Rules,
                Players = common.Players,
                CurrentTurn = common.CurrentTurn,
                Direction = common.Direction,
                ActiveReservation = common.ActiveReservation,
                CompletedTricks = common.CompletedTricks,
                ScoredTricks = common.ScoredTricks,
                CurrentTrick = common.CurrentTrick,
                Announcements = common.Announcements,
                ActiveSonderkarten = common.ActiveSonderkarten,
                TrumpEvaluator = common.TrumpEvaluator,
                PartyResolver = common.PartyResolver,
                InitialHands = common.InitialHands,
                Factory = common.Factory,
            },
            GamePhase.ArmutPartnerFinding or GamePhase.ArmutCardExchange => new ArmutFlowState
            {
                Id = common.Id,
                Phase = common.Phase,
                Rules = common.Rules,
                Players = common.Players,
                CurrentTurn = common.CurrentTurn,
                Direction = common.Direction,
                ActiveReservation = common.ActiveReservation,
                CompletedTricks = common.CompletedTricks,
                ScoredTricks = common.ScoredTricks,
                CurrentTrick = common.CurrentTrick,
                Announcements = common.Announcements,
                ActiveSonderkarten = common.ActiveSonderkarten,
                TrumpEvaluator = common.TrumpEvaluator,
                PartyResolver = common.PartyResolver,
                InitialHands = common.InitialHands,
                Factory = common.Factory,
            },
            GamePhase.Playing or GamePhase.SchwarzesSauSoloSelect => new PlayingState
            {
                Id = common.Id,
                Phase = common.Phase,
                Rules = common.Rules,
                Players = common.Players,
                CurrentTurn = common.CurrentTurn,
                Direction = common.Direction,
                ActiveReservation = common.ActiveReservation,
                CompletedTricks = common.CompletedTricks,
                ScoredTricks = common.ScoredTricks,
                CurrentTrick = common.CurrentTrick,
                Announcements = common.Announcements,
                ActiveSonderkarten = common.ActiveSonderkarten,
                TrumpEvaluator = common.TrumpEvaluator,
                PartyResolver = common.PartyResolver,
                InitialHands = common.InitialHands,
                Factory = common.Factory,
            },
            GamePhase.Scoring => new ScoringState
            {
                Id = common.Id,
                Phase = common.Phase,
                Rules = common.Rules,
                Players = common.Players,
                CurrentTurn = common.CurrentTurn,
                Direction = common.Direction,
                ActiveReservation = common.ActiveReservation,
                CompletedTricks = common.CompletedTricks,
                ScoredTricks = common.ScoredTricks,
                CurrentTrick = common.CurrentTrick,
                Announcements = common.Announcements,
                ActiveSonderkarten = common.ActiveSonderkarten,
                TrumpEvaluator = common.TrumpEvaluator,
                PartyResolver = common.PartyResolver,
                InitialHands = common.InitialHands,
                Factory = common.Factory,
            },
            GamePhase.Finished => new FinishedState
            {
                Id = common.Id,
                Phase = common.Phase,
                Rules = common.Rules,
                Players = common.Players,
                CurrentTurn = common.CurrentTurn,
                Direction = common.Direction,
                ActiveReservation = common.ActiveReservation,
                CompletedTricks = common.CompletedTricks,
                ScoredTricks = common.ScoredTricks,
                CurrentTrick = common.CurrentTrick,
                Announcements = common.Announcements,
                ActiveSonderkarten = common.ActiveSonderkarten,
                TrumpEvaluator = common.TrumpEvaluator,
                PartyResolver = common.PartyResolver,
                InitialHands = common.InitialHands,
                Factory = common.Factory,
            },
            GamePhase.Geschmissen => new GeschmissenState
            {
                Id = common.Id,
                Phase = common.Phase,
                Rules = common.Rules,
                Players = common.Players,
                CurrentTurn = common.CurrentTurn,
                Direction = common.Direction,
                ActiveReservation = common.ActiveReservation,
                CompletedTricks = common.CompletedTricks,
                ScoredTricks = common.ScoredTricks,
                CurrentTrick = common.CurrentTrick,
                Announcements = common.Announcements,
                ActiveSonderkarten = common.ActiveSonderkarten,
                TrumpEvaluator = common.TrumpEvaluator,
                PartyResolver = common.PartyResolver,
                InitialHands = common.InitialHands,
                Factory = common.Factory,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null),
        };
    }

    /// <summary>Applies a state modification, returning the resulting state.</summary>
    public GameState Apply(GameStateModification modification) =>
        modification switch
        {
            ReverseDirectionModification => this with
            {
                Direction =
                    Direction == PlayDirection.Counterclockwise
                        ? PlayDirection.Clockwise
                        : PlayDirection.Counterclockwise,
                DirectionFlipPending = false,
            },

            ScheduleDirectionFlipModification => this with { DirectionFlipPending = true },

            WithdrawAnnouncementModification m => this with
            {
                Announcements = Announcements
                    .Where(a => !(a.Player == m.Player && a.Type == m.Type))
                    .ToList(),
            },

            ActivateSonderkarteModification m => this with
            {
                ActiveSonderkarten = ActiveSonderkarten.Append(m.Type).ToList(),
            },

            RebuildTrumpEvaluatorModification => this with
            {
                TrumpEvaluator = Factory.Build(
                    ActiveReservation,
                    SilentMode,
                    ActiveSonderkarten,
                    Rules
                ),
            },

            CloseActivationWindowModification m => this with
            {
                ClosedWindows = new HashSet<SonderkarteType>(ClosedWindows) { m.Type },
            },

            AdvancePhaseModification m => TransitionToPhase(m.NewPhase),

            SetGameModeModification m => ApplySetGameMode(m),

            SetSilentGameModeModification m => ApplySetSilentGameMode(m),

            SetCurrentTurnModification m => this with { CurrentTurn = m.Player },

            DealHandsModification m => this with
            {
                Players = [.. Players.Select(p => p with { Hand = m.Hands[p.Seat] })],
                InitialHands = m.Hands,
            },

            RecordHealthDeclarationModification m => this with
            {
                HealthDeclarations = new Dictionary<PlayerSeat, bool>(HealthDeclarations)
                {
                    [m.Player] = m.HasVorbehalt,
                },
            },

            SetPendingRespondersModification m => this with
            {
                PendingReservationResponders = m.Responders,
            },

            ClearReservationDeclarationsModification => this with
            {
                ReservationDeclarations = new Dictionary<PlayerSeat, IReservation?>(),
            },

            SetArmutPlayerModification m => this with
            {
                Armut = new ArmutState(m.ArmutPlayer, null, 0, null),
            },

            SetArmutRichPlayerModification m => this with
            {
                Armut = Armut! with { RichPlayer = m.RichPlayer },
            },

            ArmutGiveTrumpsModification m => ApplyArmutGiveTrumps(m),

            SetArmutReturnedTrumpModification m => this with
            {
                Armut = Armut! with { ReturnedTrump = m.IncludedTrump },
            },

            RecordDeclarationModification m => this with
            {
                ReservationDeclarations = new Dictionary<PlayerSeat, IReservation?>(
                    ReservationDeclarations
                )
                {
                    [m.Player] = m.Declaration,
                },
            },

            UpdatePlayerHandModification m => this with
            {
                Players =
                [
                    .. Players.Select(p => p.Seat == m.Player ? p with { Hand = m.NewHand } : p),
                ],
            },

            SetCurrentTrickModification m => this with { CurrentTrick = m.Trick },

            AddCardToTrickModification m => this with
            {
                CurrentTrick = new Trick(
                    CurrentTrick!.Cards.Append(new TrickCard(m.Card, m.Player))
                ),
            },

            AddCompletedTrickModification m => this with
            {
                CompletedTricks = [.. CompletedTricks, m.Trick],
                ScoredTricks = [.. ScoredTricks, m.Result],
                CurrentTrick = null,
            },

            SetGenscherPartnerModification m => ApplySetGenscherPartner(m),

            AddAnnouncementModification m => this with
            {
                Announcements = [.. Announcements, m.Announcement],
            },

            SetVorbehaltRauskommerModification m => this with { VorbehaltRauskommer = m.Player },

            SetHochzeitForcedSoloModification => this with { HochzeitBecameForcedSolo = true },

            SetSchwarzesSauModification => this with { IsSchwarzesSau = true },

            ClearActiveSonderkartenModification => this with
            {
                ActiveSonderkarten = [],
                ClosedWindows = new HashSet<SonderkarteType>(),
            },

            ClearAnnouncementsModification => this with { Announcements = [] },

            ClearScoredTrickAwardsModification => this with
            {
                ScoredTricks = ScoredTricks.Select(r => r with { Awards = [] }).ToList(),
            },

            _ => throw new ArgumentOutOfRangeException(
                nameof(modification),
                $"Unknown modification type: {modification.GetType().Name}"
            ),
        };

    // ── Complex Apply helpers ─────────────────────────────────────────────────

    private GameState ApplySetGameMode(SetGameModeModification m)
    {
        var ctx = m.Reservation?.BuildContext();
        return this with
        {
            ActiveReservation = m.Reservation,
            GameModePlayerSeat = m.Player,
            TrumpEvaluator = ctx?.TrumpEvaluator ?? NormalTrumpEvaluator.Instance,
            PartyResolver = ctx?.PartyResolver ?? NormalPartyResolver.Instance,
        };
    }

    private GameState ApplySetSilentGameMode(SetSilentGameModeModification m)
    {
        var resolver = m.Mode?.Type switch
        {
            SilentGameModeType.KontraSolo => (IPartyResolver)
                new KontraSoloPartyResolver(m.Mode.Player),
            SilentGameModeType.StilleHochzeit => new StilleHochzeitPartyResolver(m.Mode.Player),
            _ => NormalPartyResolver.Instance,
        };
        return this with
        {
            SilentMode = m.Mode,
            TrumpEvaluator = Factory.Build(null, m.Mode, ActiveSonderkarten, Rules),
            PartyResolver = resolver,
        };
    }

    private GameState ApplyArmutGiveTrumps(ArmutGiveTrumpsModification m)
    {
        var poorState = Players.First(p => p.Seat == m.PoorPlayer);
        var richState = Players.First(p => p.Seat == m.RichPlayer);
        var trumps = poorState.Hand.Cards.Where(c => TrumpEvaluator.IsTrump(c.Type)).ToList();
        var poorNewHand = new Hands.Hand(poorState.Hand.Cards.Except(trumps).ToList());
        var richNewHand = new Hands.Hand(richState.Hand.Cards.Concat(trumps).ToList());
        return this with
        {
            Armut = Armut! with { TransferCount = trumps.Count },
            Players =
            [
                .. Players.Select(p =>
                    p.Seat == m.PoorPlayer ? p with { Hand = poorNewHand }
                    : p.Seat == m.RichPlayer ? p with { Hand = richNewHand }
                    : p
                ),
            ],
        };
    }

    private GameState ApplySetGenscherPartner(SetGenscherPartnerModification m)
    {
        if (SilentMode is not null)
            return this;

        bool teamsChanged =
            PartyResolver.ResolveParty(m.Genscher, this)
            != PartyResolver.ResolveParty(m.Partner, this);

        GenscherState? newGenscher = Genscher;
        IReadOnlyList<Announcement> newAnnouncements = Announcements;

        if (teamsChanged)
        {
            if (Genscher is null)
            {
                var rePlayers = Players
                    .Where(p => PartyResolver.ResolveParty(p.Seat, this) == Party.Re)
                    .Select(p => p.Seat)
                    .ToArray();
                newGenscher = new GenscherState(
                    TeamsChanged: true,
                    PreRePlayers: (rePlayers[0], rePlayers[1]),
                    SavedAnnouncements: Announcements
                );
                newAnnouncements = [];
            }
            else
            {
                var (orig1, orig2) = Genscher.PreRePlayers!.Value;
                bool restored =
                    (m.Genscher == orig1 || m.Genscher == orig2)
                    && (m.Partner == orig1 || m.Partner == orig2);
                if (restored && Genscher.SavedAnnouncements is not null)
                {
                    newAnnouncements = Genscher.SavedAnnouncements;
                    newGenscher = null;
                }
                else
                {
                    newGenscher = Genscher with { SavedAnnouncements = null };
                }
            }
        }

        return this with
        {
            Genscher = newGenscher,
            Announcements = newAnnouncements,
            PartyResolver = new Parties.GenscherPartyResolver(m.Genscher, m.Partner),
        };
    }

    // ── Phase transition ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new state of the subtype that corresponds to <paramref name="phase"/>,
    /// copying all current fields except <see cref="Phase"/> which is set to
    /// <paramref name="phase"/>. Called only by the <see cref="AdvancePhaseModification"/> arm.
    /// </summary>
    private GameState TransitionToPhase(GamePhase phase) =>
        phase switch
        {
            GamePhase.Dealing => new DealingState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                ActiveReservation = ActiveReservation,
                CompletedTricks = CompletedTricks,
                ScoredTricks = ScoredTricks,
                CurrentTrick = CurrentTrick,
                Announcements = Announcements,
                ActiveSonderkarten = ActiveSonderkarten,
                ClosedWindows = ClosedWindows,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                DirectionFlipPending = DirectionFlipPending,
                InitialHands = InitialHands,
                HealthDeclarations = HealthDeclarations,
                PendingReservationResponders = PendingReservationResponders,
                ReservationDeclarations = ReservationDeclarations,
                GameModePlayerSeat = GameModePlayerSeat,
                Armut = Armut,
                Genscher = Genscher,
                VorbehaltRauskommer = VorbehaltRauskommer,
                SilentMode = SilentMode,
                HochzeitBecameForcedSolo = HochzeitBecameForcedSolo,
                IsSchwarzesSau = IsSchwarzesSau,
                Factory = Factory,
            },
            GamePhase.ReservationHealthCheck
            or GamePhase.ReservationSoloCheck
            or GamePhase.ReservationArmutCheck
            or GamePhase.ReservationSchmeissenCheck
            or GamePhase.ReservationHochzeitCheck => new ReservationState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                ActiveReservation = ActiveReservation,
                CompletedTricks = CompletedTricks,
                ScoredTricks = ScoredTricks,
                CurrentTrick = CurrentTrick,
                Announcements = Announcements,
                ActiveSonderkarten = ActiveSonderkarten,
                ClosedWindows = ClosedWindows,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                DirectionFlipPending = DirectionFlipPending,
                InitialHands = InitialHands,
                HealthDeclarations = HealthDeclarations,
                PendingReservationResponders = PendingReservationResponders,
                ReservationDeclarations = ReservationDeclarations,
                GameModePlayerSeat = GameModePlayerSeat,
                Armut = Armut,
                Genscher = Genscher,
                VorbehaltRauskommer = VorbehaltRauskommer,
                SilentMode = SilentMode,
                HochzeitBecameForcedSolo = HochzeitBecameForcedSolo,
                IsSchwarzesSau = IsSchwarzesSau,
                Factory = Factory,
            },
            GamePhase.ArmutPartnerFinding or GamePhase.ArmutCardExchange => new ArmutFlowState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                ActiveReservation = ActiveReservation,
                CompletedTricks = CompletedTricks,
                ScoredTricks = ScoredTricks,
                CurrentTrick = CurrentTrick,
                Announcements = Announcements,
                ActiveSonderkarten = ActiveSonderkarten,
                ClosedWindows = ClosedWindows,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                DirectionFlipPending = DirectionFlipPending,
                InitialHands = InitialHands,
                HealthDeclarations = HealthDeclarations,
                PendingReservationResponders = PendingReservationResponders,
                ReservationDeclarations = ReservationDeclarations,
                GameModePlayerSeat = GameModePlayerSeat,
                Armut = Armut,
                Genscher = Genscher,
                VorbehaltRauskommer = VorbehaltRauskommer,
                SilentMode = SilentMode,
                HochzeitBecameForcedSolo = HochzeitBecameForcedSolo,
                IsSchwarzesSau = IsSchwarzesSau,
                Factory = Factory,
            },
            GamePhase.Playing or GamePhase.SchwarzesSauSoloSelect => new PlayingState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                ActiveReservation = ActiveReservation,
                CompletedTricks = CompletedTricks,
                ScoredTricks = ScoredTricks,
                CurrentTrick = CurrentTrick,
                Announcements = Announcements,
                ActiveSonderkarten = ActiveSonderkarten,
                ClosedWindows = ClosedWindows,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                DirectionFlipPending = DirectionFlipPending,
                InitialHands = InitialHands,
                HealthDeclarations = HealthDeclarations,
                PendingReservationResponders = PendingReservationResponders,
                ReservationDeclarations = ReservationDeclarations,
                GameModePlayerSeat = GameModePlayerSeat,
                Armut = Armut,
                Genscher = Genscher,
                VorbehaltRauskommer = VorbehaltRauskommer,
                SilentMode = SilentMode,
                HochzeitBecameForcedSolo = HochzeitBecameForcedSolo,
                IsSchwarzesSau = IsSchwarzesSau,
                Factory = Factory,
            },
            GamePhase.Scoring => new ScoringState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                ActiveReservation = ActiveReservation,
                CompletedTricks = CompletedTricks,
                ScoredTricks = ScoredTricks,
                CurrentTrick = CurrentTrick,
                Announcements = Announcements,
                ActiveSonderkarten = ActiveSonderkarten,
                ClosedWindows = ClosedWindows,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                DirectionFlipPending = DirectionFlipPending,
                InitialHands = InitialHands,
                HealthDeclarations = HealthDeclarations,
                PendingReservationResponders = PendingReservationResponders,
                ReservationDeclarations = ReservationDeclarations,
                GameModePlayerSeat = GameModePlayerSeat,
                Armut = Armut,
                Genscher = Genscher,
                VorbehaltRauskommer = VorbehaltRauskommer,
                SilentMode = SilentMode,
                HochzeitBecameForcedSolo = HochzeitBecameForcedSolo,
                IsSchwarzesSau = IsSchwarzesSau,
                Factory = Factory,
            },
            GamePhase.Finished => new FinishedState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                ActiveReservation = ActiveReservation,
                CompletedTricks = CompletedTricks,
                ScoredTricks = ScoredTricks,
                CurrentTrick = CurrentTrick,
                Announcements = Announcements,
                ActiveSonderkarten = ActiveSonderkarten,
                ClosedWindows = ClosedWindows,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                DirectionFlipPending = DirectionFlipPending,
                InitialHands = InitialHands,
                HealthDeclarations = HealthDeclarations,
                PendingReservationResponders = PendingReservationResponders,
                ReservationDeclarations = ReservationDeclarations,
                GameModePlayerSeat = GameModePlayerSeat,
                Armut = Armut,
                Genscher = Genscher,
                VorbehaltRauskommer = VorbehaltRauskommer,
                SilentMode = SilentMode,
                HochzeitBecameForcedSolo = HochzeitBecameForcedSolo,
                IsSchwarzesSau = IsSchwarzesSau,
                Factory = Factory,
            },
            GamePhase.Geschmissen => new GeschmissenState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                ActiveReservation = ActiveReservation,
                CompletedTricks = CompletedTricks,
                ScoredTricks = ScoredTricks,
                CurrentTrick = CurrentTrick,
                Announcements = Announcements,
                ActiveSonderkarten = ActiveSonderkarten,
                ClosedWindows = ClosedWindows,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                DirectionFlipPending = DirectionFlipPending,
                InitialHands = InitialHands,
                HealthDeclarations = HealthDeclarations,
                PendingReservationResponders = PendingReservationResponders,
                ReservationDeclarations = ReservationDeclarations,
                GameModePlayerSeat = GameModePlayerSeat,
                Armut = Armut,
                Genscher = Genscher,
                VorbehaltRauskommer = VorbehaltRauskommer,
                SilentMode = SilentMode,
                HochzeitBecameForcedSolo = HochzeitBecameForcedSolo,
                IsSchwarzesSau = IsSchwarzesSau,
                Factory = Factory,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null),
        };
}
