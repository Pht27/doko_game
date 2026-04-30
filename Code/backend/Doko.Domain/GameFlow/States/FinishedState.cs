using Doko.Domain.Announcements;
using Doko.Domain.Hands;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;

namespace Doko.Domain.GameFlow;

/// <summary>Terminal state — game has been scored and completed.</summary>
public sealed record FinishedState : GameState
{
    /// <summary>Each player's hand as originally dealt.</summary>
    public IReadOnlyDictionary<PlayerSeat, Hand>? InitialHands { get; init; }

    /// <summary>The active game mode reservation. Null for Normalspiel.</summary>
    public IReservation? ActiveReservation { get; init; }

    /// <summary>The player who declared the active game mode. Null for Normalspiel.</summary>
    public PlayerSeat? GameModePlayerSeat { get; init; }

    /// <summary>Armut carry-over state. Non-null only in Armut games.</summary>
    public ArmutState? Armut { get; init; }

    /// <summary>All completed tricks.</summary>
    public IReadOnlyList<Trick> CompletedTricks { get; init; } = [];

    /// <summary>
    /// Pre-computed trick results (winner + extrapunkt awards).
    /// Parallel to <see cref="CompletedTricks"/>.
    /// </summary>
    public IReadOnlyList<TrickResult> ScoredTricks { get; init; } = [];

    /// <summary>Announcements made during play.</summary>
    public IReadOnlyList<Announcement> Announcements { get; init; } = [];

    /// <summary>Sonderkarten activated during play.</summary>
    public IReadOnlyList<SonderkarteType> ActiveSonderkarten { get; init; } = [];

    /// <summary>Sonderkarten whose activation window was permanently closed.</summary>
    public IReadOnlySet<SonderkarteType> ClosedWindows { get; init; } =
        new HashSet<SonderkarteType>();

    /// <summary>Genscher-phase state. Non-null if a team-changing Genscher fired.</summary>
    public GenscherState? Genscher { get; init; }

    /// <summary>Active silent (undeclared) game mode. Null for non-silent games.</summary>
    public SilentGameMode? SilentMode { get; init; }

    /// <summary>True if a Hochzeit became a forced solo (no partner in 3 tricks).</summary>
    public bool HochzeitBecameForcedSolo { get; init; }
}
