using Doko.Domain.Announcements;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;

namespace Doko.Domain.GameFlow;

/// <summary>
/// Extension helpers for reading phase-locked fields from a <see cref="GameState"/> when
/// the caller has only the base type (e.g. extrapunkt evaluation invoked for both
/// <see cref="PlayingState"/> and <see cref="ScoringState"/>).
/// Prefer using the concrete subtype directly where possible.
/// </summary>
public static class GameStateExtensions
{
    internal static IReadOnlyList<SonderkarteType> GetActiveSonderkarten(this GameState state) =>
        state switch
        {
            PlayingState p => p.ActiveSonderkarten,
            ScoringState s => s.ActiveSonderkarten,
            FinishedState f => f.ActiveSonderkarten,
            _ => [],
        };

    public static IReadOnlyList<Trick> GetCompletedTricks(this GameState state) =>
        state switch
        {
            PlayingState p => p.CompletedTricks,
            ScoringState s => s.CompletedTricks,
            FinishedState f => f.CompletedTricks,
            _ => [],
        };

    // ── Public accessors used by application-layer query/command services ──────

    public static IReadOnlyList<TrickResult> GetScoredTricks(this GameState state) =>
        state switch
        {
            PlayingState p => p.ScoredTricks,
            ScoringState s => s.ScoredTricks,
            FinishedState f => f.ScoredTricks,
            _ => [],
        };

    public static IReadOnlyList<Announcement> GetAnnouncements(this GameState state) =>
        state switch
        {
            PlayingState p => p.Announcements,
            ScoringState s => s.Announcements,
            FinishedState f => f.Announcements,
            _ => [],
        };

    public static IReservation? GetActiveReservation(this GameState state) =>
        state switch
        {
            ReservationState r => r.ActiveReservation,
            ArmutFlowState a => a.ActiveReservation,
            PlayingState p => p.ActiveReservation,
            ScoringState s => s.ActiveReservation,
            FinishedState f => f.ActiveReservation,
            _ => null,
        };

    public static PlayerSeat? GetGameModePlayerSeat(this GameState state) =>
        state switch
        {
            ReservationState r => r.GameModePlayerSeat,
            ArmutFlowState a => a.GameModePlayerSeat,
            PlayingState p => p.GameModePlayerSeat,
            ScoringState s => s.GameModePlayerSeat,
            FinishedState f => f.GameModePlayerSeat,
            _ => null,
        };

    public static ArmutState? GetArmut(this GameState state) =>
        state switch
        {
            ArmutFlowState a => a.Armut,
            PlayingState p => p.Armut,
            ScoringState s => s.Armut,
            FinishedState f => f.Armut,
            _ => null,
        };

    public static SilentGameMode? GetSilentMode(this GameState state) =>
        state switch
        {
            PlayingState p => p.SilentMode,
            ScoringState s => s.SilentMode,
            FinishedState f => f.SilentMode,
            _ => null,
        };

    public static IReadOnlyList<PlayerSeat> GetPendingReservationResponders(this GameState state) =>
        state switch
        {
            ReservationState r => r.PendingReservationResponders,
            ArmutFlowState a => a.PendingReservationResponders,
            _ => [],
        };

    public static IReadOnlyDictionary<PlayerSeat, bool>? GetHealthDeclarations(this GameState state) =>
        state is ReservationState r ? r.HealthDeclarations : null;

    public static Trick? GetCurrentTrick(this GameState state) =>
        state is PlayingState p ? p.CurrentTrick : null;

    public static int GetCompletedTricksCount(this GameState state) =>
        state switch
        {
            PlayingState p => p.CompletedTricks.Count,
            ScoringState s => s.CompletedTricks.Count,
            FinishedState f => f.CompletedTricks.Count,
            _ => 0,
        };
}
