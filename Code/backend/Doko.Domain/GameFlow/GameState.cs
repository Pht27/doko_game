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

    public ITrumpEvaluator TrumpEvaluator { get; init; } = null!;
    public IPartyResolver PartyResolver { get; init; } = null!;

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
        var resolvedAnnouncements = announcements ?? (IReadOnlyList<Announcement>)[];
        var resolvedSonderkarten = activeSonderkarten ?? (IReadOnlyList<SonderkarteType>)[];
        var resolvedResolver = partyResolver ?? NormalPartyResolver.Instance;

        return phase switch
        {
            GamePhase.Dealing => new DealingState
            {
                Id = resolvedId,
                Phase = phase,
                Rules = resolvedRules,
                Players = players ?? (IReadOnlyList<PlayerState>)[],
                CurrentTurn = currentTurn,
                Direction = direction,
                TrumpEvaluator = resolvedEvaluator,
                PartyResolver = resolvedResolver,
                Factory = resolvedFactory,
            },
            GamePhase.ReservationHealthCheck
            or GamePhase.ReservationSoloCheck
            or GamePhase.ReservationArmutCheck
            or GamePhase.ReservationSchmeissenCheck
            or GamePhase.ReservationHochzeitCheck => new ReservationState
            {
                Id = resolvedId,
                Phase = phase,
                Rules = resolvedRules,
                Players = players ?? (IReadOnlyList<PlayerState>)[],
                CurrentTurn = currentTurn,
                Direction = direction,
                TrumpEvaluator = resolvedEvaluator,
                PartyResolver = resolvedResolver,
                Factory = resolvedFactory,
                InitialHands = initialHands,
                ActiveReservation = activeReservation,
            },
            GamePhase.ArmutPartnerFinding or GamePhase.ArmutCardExchange => new ArmutFlowState
            {
                Id = resolvedId,
                Phase = phase,
                Rules = resolvedRules,
                Players = players ?? (IReadOnlyList<PlayerState>)[],
                CurrentTurn = currentTurn,
                Direction = direction,
                TrumpEvaluator = resolvedEvaluator,
                PartyResolver = resolvedResolver,
                Factory = resolvedFactory,
                InitialHands = initialHands,
                ActiveReservation = activeReservation,
            },
            GamePhase.Playing or GamePhase.SchwarzesSauSoloSelect => new PlayingState
            {
                Id = resolvedId,
                Phase = phase,
                Rules = resolvedRules,
                Players = players ?? (IReadOnlyList<PlayerState>)[],
                CurrentTurn = currentTurn,
                Direction = direction,
                TrumpEvaluator = resolvedEvaluator,
                PartyResolver = resolvedResolver,
                Factory = resolvedFactory,
                InitialHands = initialHands,
                ActiveReservation = activeReservation,
                CompletedTricks = resolvedTricks,
                ScoredTricks = resolvedScored,
                CurrentTrick = currentTrick,
                Announcements = resolvedAnnouncements,
                ActiveSonderkarten = resolvedSonderkarten,
            },
            GamePhase.Scoring => new ScoringState
            {
                Id = resolvedId,
                Phase = phase,
                Rules = resolvedRules,
                Players = players ?? (IReadOnlyList<PlayerState>)[],
                CurrentTurn = currentTurn,
                Direction = direction,
                TrumpEvaluator = resolvedEvaluator,
                PartyResolver = resolvedResolver,
                Factory = resolvedFactory,
                InitialHands = initialHands,
                ActiveReservation = activeReservation,
                CompletedTricks = resolvedTricks,
                ScoredTricks = resolvedScored,
                Announcements = resolvedAnnouncements,
                ActiveSonderkarten = resolvedSonderkarten,
            },
            GamePhase.Finished => new FinishedState
            {
                Id = resolvedId,
                Phase = phase,
                Rules = resolvedRules,
                Players = players ?? (IReadOnlyList<PlayerState>)[],
                CurrentTurn = currentTurn,
                Direction = direction,
                TrumpEvaluator = resolvedEvaluator,
                PartyResolver = resolvedResolver,
                Factory = resolvedFactory,
                InitialHands = initialHands,
                ActiveReservation = activeReservation,
                CompletedTricks = resolvedTricks,
                ScoredTricks = resolvedScored,
                Announcements = resolvedAnnouncements,
                ActiveSonderkarten = resolvedSonderkarten,
            },
            GamePhase.Geschmissen => new GeschmissenState
            {
                Id = resolvedId,
                Phase = phase,
                Rules = resolvedRules,
                Players = players ?? (IReadOnlyList<PlayerState>)[],
                CurrentTurn = currentTurn,
                Direction = direction,
                TrumpEvaluator = resolvedEvaluator,
                PartyResolver = resolvedResolver,
                Factory = resolvedFactory,
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
                // DirectionFlipPending cleared — only lives on PlayingState
            },

            ScheduleDirectionFlipModification => ApplyScheduleDirectionFlip(),

            WithdrawAnnouncementModification m => ApplyWithdrawAnnouncement(m),

            ActivateSonderkarteModification m => ApplyActivateSonderkarte(m),

            RebuildTrumpEvaluatorModification => ApplyRebuildTrumpEvaluator(),

            CloseActivationWindowModification m => ApplyCloseActivationWindow(m),

            AdvancePhaseModification m => TransitionToPhase(m.NewPhase),

            SetGameModeModification m => ApplySetGameMode(m),

            SetSilentGameModeModification m => ApplySetSilentGameMode(m),

            SetCurrentTurnModification m => this with { CurrentTurn = m.Player },

            DealHandsModification m => this with
            {
                Players = [.. Players.Select(p => p with { Hand = m.Hands[p.Seat] })],
            },

            RecordHealthDeclarationModification m => ApplyRecordHealthDeclaration(m),

            SetPendingRespondersModification m => ApplySetPendingResponders(m),

            ClearReservationDeclarationsModification => ApplyClearReservationDeclarations(),

            SetArmutPlayerModification m => ApplySetArmutPlayer(m),

            SetArmutRichPlayerModification m => ApplySetArmutRichPlayer(m),

            ArmutGiveTrumpsModification m => ApplyArmutGiveTrumps(m),

            SetArmutReturnedTrumpModification m => ApplySetArmutReturnedTrump(m),

            RecordDeclarationModification m => ApplyRecordDeclaration(m),

            UpdatePlayerHandModification m => this with
            {
                Players =
                [
                    .. Players.Select(p => p.Seat == m.Player ? p with { Hand = m.NewHand } : p),
                ],
            },

            SetCurrentTrickModification m => ApplySetCurrentTrick(m),

            AddCardToTrickModification m => ApplyAddCardToTrick(m),

            AddCompletedTrickModification m => ApplyAddCompletedTrick(m),

            SetGenscherPartnerModification m => ApplySetGenscherPartner(m),

            AddAnnouncementModification m => ApplyAddAnnouncement(m),

            SetVorbehaltRauskommerModification m => ApplySetVorbehaltRauskommer(m),

            SetHochzeitForcedSoloModification => ApplySetHochzeitForcedSolo(),

            SetSchwarzesSauModification => ApplySetSchwarzesSau(),

            ClearActiveSonderkartenModification => ApplyClearActiveSonderkarten(),

            ClearAnnouncementsModification => ApplyClearAnnouncements(),

            ClearScoredTrickAwardsModification => ApplyClearScoredTrickAwards(),

            _ => throw new ArgumentOutOfRangeException(
                nameof(modification),
                $"Unknown modification type: {modification.GetType().Name}"
            ),
        };

    // ── Apply helpers — modifications that read/write phase-locked fields ────

    private GameState ApplyScheduleDirectionFlip()
    {
        if (this is PlayingState p)
            return p with { DirectionFlipPending = true };
        throw new InvalidOperationException(
            $"ScheduleDirectionFlipModification requires PlayingState, got {GetType().Name}"
        );
    }

    private GameState ApplyWithdrawAnnouncement(WithdrawAnnouncementModification m)
    {
        return this switch
        {
            PlayingState p => p with
            {
                Announcements = p
                    .Announcements.Where(a => !(a.Player == m.Player && a.Type == m.Type))
                    .ToList(),
            },
            ScoringState s => s with
            {
                Announcements = s
                    .Announcements.Where(a => !(a.Player == m.Player && a.Type == m.Type))
                    .ToList(),
            },
            _ => throw new InvalidOperationException(
                $"WithdrawAnnouncementModification requires PlayingState or ScoringState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplyActivateSonderkarte(ActivateSonderkarteModification m)
    {
        return this switch
        {
            PlayingState p => p with
            {
                ActiveSonderkarten = p.ActiveSonderkarten.Append(m.Type).ToList(),
            },
            ScoringState s => s with
            {
                ActiveSonderkarten = s.ActiveSonderkarten.Append(m.Type).ToList(),
            },
            _ => throw new InvalidOperationException(
                $"ActivateSonderkarteModification requires PlayingState or ScoringState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplyRebuildTrumpEvaluator()
    {
        return this switch
        {
            PlayingState p => p with
            {
                TrumpEvaluator = Factory.Build(
                    p.ActiveReservation,
                    p.SilentMode,
                    p.ActiveSonderkarten,
                    Rules
                ),
            },
            ScoringState s => s with
            {
                TrumpEvaluator = Factory.Build(
                    s.ActiveReservation,
                    s.SilentMode,
                    s.ActiveSonderkarten,
                    Rules
                ),
            },
            ReservationState r => r with
            {
                TrumpEvaluator = Factory.Build(r.ActiveReservation, null, [], Rules),
            },
            ArmutFlowState a => a with
            {
                TrumpEvaluator = Factory.Build(a.ActiveReservation, null, [], Rules),
            },
            _ => this with
            {
                TrumpEvaluator = Factory.Build(null, null, [], Rules),
            },
        };
    }

    private GameState ApplyCloseActivationWindow(CloseActivationWindowModification m)
    {
        return this switch
        {
            PlayingState p => p with
            {
                ClosedWindows = new HashSet<SonderkarteType>(p.ClosedWindows) { m.Type },
            },
            ScoringState s => s with
            {
                ClosedWindows = new HashSet<SonderkarteType>(s.ClosedWindows) { m.Type },
            },
            _ => throw new InvalidOperationException(
                $"CloseActivationWindowModification requires PlayingState or ScoringState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplySetGameMode(SetGameModeModification m)
    {
        var ctx = m.Reservation?.BuildContext();
        var evaluator = ctx?.TrumpEvaluator ?? NormalTrumpEvaluator.Instance;
        var resolver = ctx?.PartyResolver ?? NormalPartyResolver.Instance;

        return this switch
        {
            ReservationState r => r with
            {
                ActiveReservation = m.Reservation,
                GameModePlayerSeat = m.Player,
                TrumpEvaluator = evaluator,
                PartyResolver = resolver,
            },
            ArmutFlowState a => a with
            {
                ActiveReservation = m.Reservation,
                GameModePlayerSeat = m.Player,
                TrumpEvaluator = evaluator,
                PartyResolver = resolver,
            },
            PlayingState p => p with
            {
                ActiveReservation = m.Reservation,
                GameModePlayerSeat = m.Player,
                TrumpEvaluator = evaluator,
                PartyResolver = resolver,
            },
            ScoringState s => s with
            {
                ActiveReservation = m.Reservation,
                GameModePlayerSeat = m.Player,
                TrumpEvaluator = evaluator,
                PartyResolver = resolver,
            },
            FinishedState f => f with
            {
                ActiveReservation = m.Reservation,
                GameModePlayerSeat = m.Player,
                TrumpEvaluator = evaluator,
                PartyResolver = resolver,
            },
            _ => throw new InvalidOperationException(
                $"SetGameModeModification requires a state that carries ActiveReservation, got {GetType().Name}"
            ),
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

        return this switch
        {
            ReservationState r => r with
            {
                TrumpEvaluator = Factory.Build(null, m.Mode, [], Rules),
                PartyResolver = resolver,
            },
            PlayingState p => p with
            {
                SilentMode = m.Mode,
                TrumpEvaluator = Factory.Build(null, m.Mode, p.ActiveSonderkarten, Rules),
                PartyResolver = resolver,
            },
            ScoringState s => s with
            {
                SilentMode = m.Mode,
                TrumpEvaluator = Factory.Build(null, m.Mode, s.ActiveSonderkarten, Rules),
                PartyResolver = resolver,
            },
            _ => throw new InvalidOperationException(
                $"SetSilentGameModeModification requires ReservationState, PlayingState, or ScoringState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplyRecordHealthDeclaration(RecordHealthDeclarationModification m)
    {
        if (this is not ReservationState r)
            throw new InvalidOperationException(
                $"RecordHealthDeclarationModification requires ReservationState, got {GetType().Name}"
            );
        return r with
        {
            HealthDeclarations = new Dictionary<PlayerSeat, bool>(r.HealthDeclarations)
            {
                [m.Player] = m.HasVorbehalt,
            },
        };
    }

    private GameState ApplySetPendingResponders(SetPendingRespondersModification m)
    {
        return this switch
        {
            ReservationState r => r with { PendingReservationResponders = m.Responders },
            ArmutFlowState a => a with { PendingReservationResponders = m.Responders },
            _ => throw new InvalidOperationException(
                $"SetPendingRespondersModification requires ReservationState or ArmutFlowState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplyClearReservationDeclarations()
    {
        if (this is not ReservationState r)
            throw new InvalidOperationException(
                $"ClearReservationDeclarationsModification requires ReservationState, got {GetType().Name}"
            );
        return r with
        {
            ReservationDeclarations = new Dictionary<PlayerSeat, IReservation?>(),
        };
    }

    private GameState ApplySetArmutPlayer(SetArmutPlayerModification m)
    {
        return this switch
        {
            ReservationState r => r with { },
            ArmutFlowState a => a with
            {
                Armut = new ArmutState(m.ArmutPlayer, null, 0, null),
            },
            PlayingState p => p with
            {
                Armut = new ArmutState(m.ArmutPlayer, null, 0, null),
            },
            _ => throw new InvalidOperationException(
                $"SetArmutPlayerModification requires ArmutFlowState or PlayingState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplySetArmutRichPlayer(SetArmutRichPlayerModification m)
    {
        return this switch
        {
            ArmutFlowState a => a with
            {
                Armut = a.Armut! with { RichPlayer = m.RichPlayer },
            },
            PlayingState p => p with
            {
                Armut = p.Armut! with { RichPlayer = m.RichPlayer },
            },
            _ => throw new InvalidOperationException(
                $"SetArmutRichPlayerModification requires ArmutFlowState or PlayingState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplyArmutGiveTrumps(ArmutGiveTrumpsModification m)
    {
        var poorState = Players.First(p => p.Seat == m.PoorPlayer);
        var richState = Players.First(p => p.Seat == m.RichPlayer);
        var trumps = poorState.Hand.Cards.Where(c => TrumpEvaluator.IsTrump(c.Type)).ToList();
        var poorNewHand = new Hands.Hand(poorState.Hand.Cards.Except(trumps).ToList());
        var richNewHand = new Hands.Hand(richState.Hand.Cards.Concat(trumps).ToList());

        var newPlayers = Players
            .Select(p =>
                p.Seat == m.PoorPlayer ? p with { Hand = poorNewHand }
                : p.Seat == m.RichPlayer ? p with { Hand = richNewHand }
                : p
            )
            .ToList();

        return this switch
        {
            ArmutFlowState a => a with
            {
                Armut = a.Armut! with { TransferCount = trumps.Count },
                Players = [.. newPlayers],
            },
            PlayingState p => p with
            {
                Armut = p.Armut! with { TransferCount = trumps.Count },
                Players = [.. newPlayers],
            },
            _ => throw new InvalidOperationException(
                $"ArmutGiveTrumpsModification requires ArmutFlowState or PlayingState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplySetArmutReturnedTrump(SetArmutReturnedTrumpModification m)
    {
        return this switch
        {
            ArmutFlowState a => a with
            {
                Armut = a.Armut! with { ReturnedTrump = m.IncludedTrump },
            },
            PlayingState p => p with
            {
                Armut = p.Armut! with { ReturnedTrump = m.IncludedTrump },
            },
            _ => throw new InvalidOperationException(
                $"SetArmutReturnedTrumpModification requires ArmutFlowState or PlayingState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplyRecordDeclaration(RecordDeclarationModification m)
    {
        if (this is not ReservationState r)
            throw new InvalidOperationException(
                $"RecordDeclarationModification requires ReservationState, got {GetType().Name}"
            );
        return r with
        {
            ReservationDeclarations = new Dictionary<PlayerSeat, IReservation?>(
                r.ReservationDeclarations
            )
            {
                [m.Player] = m.Declaration,
            },
        };
    }

    private GameState ApplySetCurrentTrick(SetCurrentTrickModification m)
    {
        if (this is not PlayingState p)
            throw new InvalidOperationException(
                $"SetCurrentTrickModification requires PlayingState, got {GetType().Name}"
            );
        return p with { CurrentTrick = m.Trick };
    }

    private GameState ApplyAddCardToTrick(AddCardToTrickModification m)
    {
        if (this is not PlayingState p)
            throw new InvalidOperationException(
                $"AddCardToTrickModification requires PlayingState, got {GetType().Name}"
            );
        return p with
        {
            CurrentTrick = new Trick(
                p.CurrentTrick!.Cards.Append(new TrickCard(m.Card, m.Player))
            ),
        };
    }

    private GameState ApplyAddCompletedTrick(AddCompletedTrickModification m)
    {
        if (this is not PlayingState p)
            throw new InvalidOperationException(
                $"AddCompletedTrickModification requires PlayingState, got {GetType().Name}"
            );
        return p with
        {
            CompletedTricks = [.. p.CompletedTricks, m.Trick],
            ScoredTricks = [.. p.ScoredTricks, m.Result],
            CurrentTrick = null,
        };
    }

    private GameState ApplySetGenscherPartner(SetGenscherPartnerModification m)
    {
        if (this is not PlayingState p)
            throw new InvalidOperationException(
                $"SetGenscherPartnerModification requires PlayingState, got {GetType().Name}"
            );

        if (p.SilentMode is not null)
            return this;

        bool teamsChanged =
            PartyResolver.ResolveParty(m.Genscher, this)
            != PartyResolver.ResolveParty(m.Partner, this);

        GenscherState? newGenscher = p.Genscher;
        IReadOnlyList<Announcement> newAnnouncements = p.Announcements;

        if (teamsChanged)
        {
            if (p.Genscher is null)
            {
                var rePlayers = Players
                    .Where(player => PartyResolver.ResolveParty(player.Seat, this) == Party.Re)
                    .Select(player => player.Seat)
                    .ToArray();
                newGenscher = new GenscherState(
                    TeamsChanged: true,
                    PreRePlayers: (rePlayers[0], rePlayers[1]),
                    SavedAnnouncements: p.Announcements
                );
                newAnnouncements = [];
            }
            else
            {
                var (orig1, orig2) = p.Genscher.PreRePlayers!.Value;
                bool restored =
                    (m.Genscher == orig1 || m.Genscher == orig2)
                    && (m.Partner == orig1 || m.Partner == orig2);
                if (restored && p.Genscher.SavedAnnouncements is not null)
                {
                    newAnnouncements = p.Genscher.SavedAnnouncements;
                    newGenscher = null;
                }
                else
                {
                    newGenscher = p.Genscher with { SavedAnnouncements = null };
                }
            }
        }

        return p with
        {
            Genscher = newGenscher,
            Announcements = newAnnouncements,
            PartyResolver = new Parties.GenscherPartyResolver(m.Genscher, m.Partner),
        };
    }

    private GameState ApplyAddAnnouncement(AddAnnouncementModification m)
    {
        return this switch
        {
            PlayingState p => p with
            {
                Announcements = [.. p.Announcements, m.Announcement],
            },
            ScoringState s => s with
            {
                Announcements = [.. s.Announcements, m.Announcement],
            },
            _ => throw new InvalidOperationException(
                $"AddAnnouncementModification requires PlayingState or ScoringState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplySetVorbehaltRauskommer(SetVorbehaltRauskommerModification m)
    {
        return this switch
        {
            ReservationState r => r with { VorbehaltRauskommer = m.Player },
            ArmutFlowState a => a with { VorbehaltRauskommer = m.Player },
            _ => throw new InvalidOperationException(
                $"SetVorbehaltRauskommerModification requires ReservationState or ArmutFlowState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplySetHochzeitForcedSolo()
    {
        return this switch
        {
            PlayingState p => p with { HochzeitBecameForcedSolo = true },
            ScoringState s => s with { HochzeitBecameForcedSolo = true },
            _ => throw new InvalidOperationException(
                $"SetHochzeitForcedSoloModification requires PlayingState or ScoringState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplySetSchwarzesSau()
    {
        if (this is not PlayingState p)
            throw new InvalidOperationException(
                $"SetSchwarzesSauModification requires PlayingState, got {GetType().Name}"
            );
        return p with { IsSchwarzesSau = true };
    }

    private GameState ApplyClearActiveSonderkarten()
    {
        return this switch
        {
            PlayingState p => p with
            {
                ActiveSonderkarten = [],
                ClosedWindows = new HashSet<SonderkarteType>(),
            },
            ScoringState s => s with
            {
                ActiveSonderkarten = [],
                ClosedWindows = new HashSet<SonderkarteType>(),
            },
            _ => throw new InvalidOperationException(
                $"ClearActiveSonderkartenModification requires PlayingState or ScoringState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplyClearAnnouncements()
    {
        return this switch
        {
            PlayingState p => p with { Announcements = [] },
            ScoringState s => s with { Announcements = [] },
            _ => throw new InvalidOperationException(
                $"ClearAnnouncementsModification requires PlayingState or ScoringState, got {GetType().Name}"
            ),
        };
    }

    private GameState ApplyClearScoredTrickAwards()
    {
        return this switch
        {
            PlayingState p => p with
            {
                ScoredTricks = p.ScoredTricks.Select(r => r with { Awards = [] }).ToList(),
            },
            ScoringState s => s with
            {
                ScoredTricks = s.ScoredTricks.Select(r => r with { Awards = [] }).ToList(),
            },
            _ => throw new InvalidOperationException(
                $"ClearScoredTrickAwardsModification requires PlayingState or ScoringState, got {GetType().Name}"
            ),
        };
    }

    // ── Phase transition ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new state of the subtype that corresponds to <paramref name="phase"/>,
    /// copying all applicable fields from the current state.
    /// </summary>
    private GameState TransitionToPhase(GamePhase phase)
    {
        // Extract fields from the current subtype where available
        IReservation? activeReservation = null;
        PlayerSeat? gameModePlayerSeat = null;
        IReadOnlyDictionary<PlayerSeat, Hand>? initialHands = null;
        ArmutState? armut = null;
        IReadOnlyList<PlayerSeat> pendingResponders = [];
        PlayerSeat vorbehaltRauskommer = default;
        IReadOnlyDictionary<PlayerSeat, bool> healthDeclarations =
            new Dictionary<PlayerSeat, bool>();
        IReadOnlyDictionary<PlayerSeat, IReservation?> reservationDeclarations =
            new Dictionary<PlayerSeat, IReservation?>();
        IReadOnlyList<Trick> completedTricks = [];
        IReadOnlyList<TrickResult> scoredTricks = [];
        IReadOnlyList<Announcement> announcements = [];
        IReadOnlyList<SonderkarteType> activeSonderkarten = [];
        IReadOnlySet<SonderkarteType> closedWindows = new HashSet<SonderkarteType>();
        Trick? currentTrick = null;
        bool directionFlipPending = false;
        GenscherState? genscher = null;
        SilentGameMode? silentMode = null;
        bool hochzeitBecameForcedSolo = false;
        bool isSchwarzesSau = false;

        switch (this)
        {
            case ReservationState r:
                activeReservation = r.ActiveReservation;
                gameModePlayerSeat = r.GameModePlayerSeat;
                initialHands = r.InitialHands;
                pendingResponders = r.PendingReservationResponders;
                vorbehaltRauskommer = r.VorbehaltRauskommer;
                healthDeclarations = r.HealthDeclarations;
                reservationDeclarations = r.ReservationDeclarations;
                break;
            case ArmutFlowState a:
                activeReservation = a.ActiveReservation;
                gameModePlayerSeat = a.GameModePlayerSeat;
                initialHands = a.InitialHands;
                pendingResponders = a.PendingReservationResponders;
                vorbehaltRauskommer = a.VorbehaltRauskommer;
                armut = a.Armut;
                break;
            case PlayingState p:
                activeReservation = p.ActiveReservation;
                gameModePlayerSeat = p.GameModePlayerSeat;
                initialHands = p.InitialHands;
                armut = p.Armut;
                completedTricks = p.CompletedTricks;
                scoredTricks = p.ScoredTricks;
                currentTrick = p.CurrentTrick;
                announcements = p.Announcements;
                activeSonderkarten = p.ActiveSonderkarten;
                closedWindows = p.ClosedWindows;
                directionFlipPending = p.DirectionFlipPending;
                genscher = p.Genscher;
                silentMode = p.SilentMode;
                hochzeitBecameForcedSolo = p.HochzeitBecameForcedSolo;
                isSchwarzesSau = p.IsSchwarzesSau;
                break;
            case ScoringState s:
                activeReservation = s.ActiveReservation;
                gameModePlayerSeat = s.GameModePlayerSeat;
                initialHands = s.InitialHands;
                armut = s.Armut;
                completedTricks = s.CompletedTricks;
                scoredTricks = s.ScoredTricks;
                announcements = s.Announcements;
                activeSonderkarten = s.ActiveSonderkarten;
                closedWindows = s.ClosedWindows;
                genscher = s.Genscher;
                silentMode = s.SilentMode;
                hochzeitBecameForcedSolo = s.HochzeitBecameForcedSolo;
                break;
            case FinishedState f:
                activeReservation = f.ActiveReservation;
                gameModePlayerSeat = f.GameModePlayerSeat;
                initialHands = f.InitialHands;
                armut = f.Armut;
                completedTricks = f.CompletedTricks;
                scoredTricks = f.ScoredTricks;
                announcements = f.Announcements;
                activeSonderkarten = f.ActiveSonderkarten;
                closedWindows = f.ClosedWindows;
                genscher = f.Genscher;
                silentMode = f.SilentMode;
                hochzeitBecameForcedSolo = f.HochzeitBecameForcedSolo;
                break;
        }

        return phase switch
        {
            GamePhase.Dealing => new DealingState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
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
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                Factory = Factory,
                ActiveReservation = activeReservation,
                GameModePlayerSeat = gameModePlayerSeat,
                InitialHands = initialHands,
                PendingReservationResponders = pendingResponders,
                VorbehaltRauskommer = vorbehaltRauskommer,
                HealthDeclarations = healthDeclarations,
                ReservationDeclarations = reservationDeclarations,
            },
            GamePhase.ArmutPartnerFinding or GamePhase.ArmutCardExchange => new ArmutFlowState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                Factory = Factory,
                ActiveReservation = activeReservation,
                GameModePlayerSeat = gameModePlayerSeat,
                InitialHands = initialHands,
                PendingReservationResponders = pendingResponders,
                VorbehaltRauskommer = vorbehaltRauskommer,
                Armut = armut,
            },
            GamePhase.Playing or GamePhase.SchwarzesSauSoloSelect => new PlayingState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                Factory = Factory,
                ActiveReservation = activeReservation,
                GameModePlayerSeat = gameModePlayerSeat,
                InitialHands = initialHands,
                Armut = armut,
                CompletedTricks = completedTricks,
                ScoredTricks = scoredTricks,
                CurrentTrick = currentTrick,
                Announcements = announcements,
                ActiveSonderkarten = activeSonderkarten,
                ClosedWindows = closedWindows,
                DirectionFlipPending = directionFlipPending,
                Genscher = genscher,
                SilentMode = silentMode,
                HochzeitBecameForcedSolo = hochzeitBecameForcedSolo,
                IsSchwarzesSau = isSchwarzesSau,
            },
            GamePhase.Scoring => new ScoringState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                Factory = Factory,
                ActiveReservation = activeReservation,
                GameModePlayerSeat = gameModePlayerSeat,
                InitialHands = initialHands,
                Armut = armut,
                CompletedTricks = completedTricks,
                ScoredTricks = scoredTricks,
                Announcements = announcements,
                ActiveSonderkarten = activeSonderkarten,
                ClosedWindows = closedWindows,
                Genscher = genscher,
                SilentMode = silentMode,
                HochzeitBecameForcedSolo = hochzeitBecameForcedSolo,
            },
            GamePhase.Finished => new FinishedState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                Factory = Factory,
                ActiveReservation = activeReservation,
                GameModePlayerSeat = gameModePlayerSeat,
                InitialHands = initialHands,
                Armut = armut,
                CompletedTricks = completedTricks,
                ScoredTricks = scoredTricks,
                Announcements = announcements,
                ActiveSonderkarten = activeSonderkarten,
                ClosedWindows = closedWindows,
                Genscher = genscher,
                SilentMode = silentMode,
                HochzeitBecameForcedSolo = hochzeitBecameForcedSolo,
            },
            GamePhase.Geschmissen => new GeschmissenState
            {
                Id = Id,
                Phase = phase,
                Rules = Rules,
                Players = Players,
                CurrentTurn = CurrentTurn,
                Direction = Direction,
                TrumpEvaluator = TrumpEvaluator,
                PartyResolver = PartyResolver,
                Factory = Factory,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null),
        };
    }
}
