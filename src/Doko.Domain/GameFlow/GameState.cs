using Doko.Domain.Announcements;
using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Rules;
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

    public ITrumpEvaluator TrumpEvaluator { get; private set; }
    public IPartyResolver PartyResolver { get; private set; }

    /// <summary>
    /// Each player's hand as originally dealt. Set once when dealing completes, never mutated.
    /// Used by sonderkarte eligibility checks that need to know what a player originally held
    /// (e.g. Superschweinchen: player originally held both ♦10, but may have already played the first).
    /// Null before dealing phase completes.
    /// </summary>
    public IReadOnlyDictionary<PlayerId, Hand>? InitialHands { get; private set; }

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
        RuleSet?                                 rules               = null,
        IReadOnlyList<PlayerState>?              players             = null,
        PlayerId                                 currentTurn         = default,
        ITrumpEvaluator?                         trumpEvaluator      = null,
        IPartyResolver?                          partyResolver       = null,
        PlayDirection                            direction           = PlayDirection.Counterclockwise,
        IReservation?                            activeReservation   = null,
        IReadOnlyList<Trick>?                    completedTricks     = null,
        Trick?                                   currentTrick        = null,
        IReadOnlyList<Announcement>?             announcements       = null,
        IReadOnlyList<SonderkarteType>?          activeSonderkarten  = null,
        IReadOnlyDictionary<PlayerId, Hand>?     initialHands        = null,
        GameId                                   id                  = default) => new GameState
        {
            Id                 = id.Value == Guid.Empty ? GameId.New() : id,
            Phase              = GamePhase.Playing,
            Rules              = rules              ?? RuleSet.Default(),
            Players            = players            ?? [],
            CurrentTurn        = currentTurn,
            Direction          = direction,
            ActiveReservation  = activeReservation,
            CompletedTricks    = completedTricks    ?? [],
            CurrentTrick       = currentTrick,
            Announcements      = announcements      ?? [],
            ActiveSonderkarten = activeSonderkarten ?? [],
            TrumpEvaluator     = trumpEvaluator     ?? NormalTrumpEvaluator.Instance,
            PartyResolver      = partyResolver      ?? NormalPartyResolver.Instance,
            InitialHands       = initialHands,
        };

    /// <summary>Determines the next player in the given play direction.</summary>
    public PlayerId NextPlayer(PlayerId current, PlayDirection direction)
    {
        var currentPlayer = Players.First(p => p.Id == current);
        int seatIndex = (int)currentPlayer.Seat;
        int nextIndex = direction == PlayDirection.Counterclockwise
            ? (seatIndex + 1) % 4
            : (seatIndex + 3) % 4;
        return Players.First(p => (int)p.Seat == nextIndex).Id;
    }

    /// <summary>Applies a state modification. The only place mutations occur.</summary>
    public void Apply(GameStateModification modification)
    {
        switch (modification)
        {
            case ReverseDirectionModification:
                Direction = Direction == PlayDirection.Counterclockwise
                    ? PlayDirection.Clockwise
                    : PlayDirection.Counterclockwise;
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
                RebuildTrumpEvaluator();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(modification),
                    $"Unknown modification type: {modification.GetType().Name}");
        }
    }

    private void RebuildTrumpEvaluator()
    {
        var baseEvaluator = ActiveReservation?.Apply().TrumpEvaluator ?? NormalTrumpEvaluator.Instance;

        var activeSet   = ActiveSonderkarten.ToHashSet();
        var suppressed  = SonderkarteRegistry.GetEnabled(Rules)
            .Where(s => activeSet.Contains(s.Type) && s.Suppresses.HasValue)
            .Select(s => s.Suppresses!.Value)
            .ToHashSet();

        var modifiers = SonderkarteRegistry.GetEnabled(Rules)
            .Where(s => activeSet.Contains(s.Type)
                     && !suppressed.Contains(s.Type)
                     && s.RankingModifier is not null)
            .Select(s => s.RankingModifier!)
            .ToList();

        TrumpEvaluator = modifiers.Count > 0
            ? new StandardSonderkarteDecorator(baseEvaluator, modifiers)
            : baseEvaluator;
    }
}
