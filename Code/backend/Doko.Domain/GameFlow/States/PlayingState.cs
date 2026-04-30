using Doko.Domain.Announcements;
using Doko.Domain.Hands;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;

namespace Doko.Domain.GameFlow;

/// <summary>
/// Active play cluster. <see cref="GameState.Phase"/> is either
/// <see cref="GamePhase.Playing"/> or <see cref="GamePhase.SchwarzesSauSoloSelect"/>.
/// </summary>
public sealed record PlayingState : GameState
{
    /// <summary>Each player's hand as originally dealt.</summary>
    public IReadOnlyDictionary<PlayerSeat, Hand>? InitialHands { get; init; }

    /// <summary>The active game mode reservation. Null for Normalspiel.</summary>
    public IReservation? ActiveReservation { get; init; }

    /// <summary>The player who declared the active game mode. Null for Normalspiel.</summary>
    public PlayerSeat? GameModePlayerSeat { get; init; }

    /// <summary>Armut-phase carry-over state. Non-null only in Armut games.</summary>
    public ArmutState? Armut { get; init; }

    /// <summary>Tricks completed so far.</summary>
    public IReadOnlyList<Trick> CompletedTricks { get; init; } = [];

    /// <summary>The trick currently being played. Null between tricks.</summary>
    public Trick? CurrentTrick { get; init; }

    /// <summary>
    /// Pre-computed trick results (winner + extrapunkt awards) appended at trick completion time.
    /// Parallel to <see cref="CompletedTricks"/>.
    /// </summary>
    public IReadOnlyList<TrickResult> ScoredTricks { get; init; } = [];

    /// <summary>Announcements made so far.</summary>
    public IReadOnlyList<Announcement> Announcements { get; init; } = [];

    /// <summary>Sonderkarten activated so far.</summary>
    public IReadOnlyList<SonderkarteType> ActiveSonderkarten { get; init; } = [];

    /// <summary>
    /// Sonderkarten whose activation window has permanently closed.
    /// </summary>
    public IReadOnlySet<SonderkarteType> ClosedWindows { get; init; } =
        new HashSet<SonderkarteType>();

    /// <summary>
    /// True when a direction reversal was activated mid-trick and should take effect at the
    /// start of the next trick.
    /// </summary>
    public bool DirectionFlipPending { get; init; }

    /// <summary>
    /// Genscher-phase state. Non-null once a team-changing Genscher has fired.
    /// </summary>
    public GenscherState? Genscher { get; init; }

    /// <summary>
    /// Active silent (undeclared) game mode. Null in all other game modes.
    /// </summary>
    public SilentGameMode? SilentMode { get; init; }

    /// <summary>
    /// True once a Hochzeit failed to find a partner in 3 qualifying tricks and became a forced solo.
    /// </summary>
    public bool HochzeitBecameForcedSolo { get; init; }

    /// <summary>
    /// True when the game is running as Schwarze Sau (Armut with no partner found).
    /// </summary>
    public bool IsSchwarzesSau { get; init; }
}
