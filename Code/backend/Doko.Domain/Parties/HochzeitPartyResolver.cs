using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Scoring;

namespace Doko.Domain.Parties;

/// <summary>
/// Hochzeit party resolution. The Hochzeit player is always Re.
/// The first player (other than the Hochzeit player) to win a qualifying trick becomes
/// the second Re member. After 3 qualifying tricks with no partner found, it becomes a
/// Stille Hochzeit (solo: Hochzeit player is Re alone).
/// </summary>
public sealed class HochzeitPartyResolver : IPartyResolver
{
    private readonly PlayerSeat _hochzeitPlayer;
    private readonly HochzeitCondition _condition;

    public HochzeitPartyResolver(PlayerSeat hochzeitPlayer, HochzeitCondition condition)
    {
        _hochzeitPlayer = hochzeitPlayer;
        _condition = condition;
    }

    public Party? ResolveParty(PlayerSeat player, GameState state)
    {
        if (player == _hochzeitPlayer)
            return Party.Re;

        int? findungsstich = FindFindungstrickIndex(state);
        if (findungsstich.HasValue)
        {
            var scored = GetScoredTricks(state);
            return player == scored![findungsstich.Value].Winner
                ? Party.Re
                : Party.Kontra;
        }

        // After 3 tricks with no partner → Stille Hochzeit (solo)
        var completed = GetCompletedTricks(state);
        if (completed is not null && completed.Count >= 3)
            return Party.Kontra;

        return null; // Still searching for partner
    }

    public bool IsFullyResolved(GameState state)
    {
        var completed = GetCompletedTricks(state);
        return FindFindungstrickIndex(state).HasValue
            || (completed is not null && completed.Count >= 3);
    }

    /// <summary>
    /// Returns the announcement deadline relative to the Findungsstich.
    /// No announcements are allowed before the Findungsstich (returns null).
    /// After the Findungsstich at trick index K, the deadline is K*4+5
    /// (= before the 2nd card of the trick after the Findungsstich).
    /// </summary>
    public int? AnnouncementBaseDeadline(GameState state)
    {
        int? i = FindFindungstrickIndex(state);
        return i.HasValue ? i.Value * 4 + 5 : null;
    }

    private int? FindFindungstrickIndex(GameState state)
    {
        var completed = GetCompletedTricks(state);
        var scored = GetScoredTricks(state);
        if (completed is null || scored is null)
            return null;

        for (int i = 0; i < completed.Count && i < 3; i++)
        {
            var winner = scored[i].Winner;
            if (winner != _hochzeitPlayer && Qualifies(completed[i], state))
                return i;
        }
        return null;
    }

    private bool Qualifies(Tricks.Trick trick, GameState state) =>
        _condition switch
        {
            HochzeitCondition.FirstTrick => true,
            HochzeitCondition.FirstFehlTrick => !state.TrumpEvaluator.IsTrump(
                trick.Cards[0].Card.Type
            ),
            HochzeitCondition.FirstTrumpTrick => state.TrumpEvaluator.IsTrump(
                trick.Cards[0].Card.Type
            ),
            _ => false,
        };

    private static IReadOnlyList<Tricks.Trick>? GetCompletedTricks(GameState state) =>
        state switch
        {
            PlayingState p => p.CompletedTricks,
            ScoringState s => s.CompletedTricks,
            FinishedState f => f.CompletedTricks,
            _ => null,
        };

    private static IReadOnlyList<Scoring.TrickResult>? GetScoredTricks(GameState state) =>
        state switch
        {
            PlayingState p => p.ScoredTricks,
            ScoringState s => s.ScoredTricks,
            FinishedState f => f.ScoredTricks,
            _ => null,
        };
}
