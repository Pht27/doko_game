using Doko.Domain.GameFlow;
using Doko.Domain.Players;

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

        for (int i = 0; i < state.CompletedTricks.Count; i++)
        {
            var winner = state.ScoredTricks[i].Winner;
            if (winner != _hochzeitPlayer && Qualifies(state.CompletedTricks[i], state) && i < 3)
                return player == winner ? Party.Re : Party.Kontra;
        }

        // After 3 tricks with no partner → Stille Hochzeit (solo)
        if (state.CompletedTricks.Count >= 3)
            return Party.Kontra;

        return null; // Still searching for partner
    }

    public bool IsFullyResolved(GameState state)
    {
        for (int i = 0; i < state.CompletedTricks.Count; i++)
        {
            var winner = state.ScoredTricks[i].Winner;
            if (winner != _hochzeitPlayer && Qualifies(state.CompletedTricks[i], state) && i < 3)
                return true;
        }
        return state.CompletedTricks.Count >= 3;
    }

    /// <summary>
    /// Returns the announcement deadline relative to the Findungsstich.
    /// No announcements are allowed before the Findungsstich (returns null).
    /// After the Findungsstich at trick index K, the deadline is K*4+5
    /// (= before the 2nd card of the trick after the Findungsstich).
    /// </summary>
    public int? AnnouncementBaseDeadline(GameState state)
    {
        for (int i = 0; i < state.CompletedTricks.Count; i++)
        {
            if (!Qualifies(state.CompletedTricks[i], state))
                continue;
            var winner = state.ScoredTricks[i].Winner;
            if (winner != _hochzeitPlayer)
                return i * 4 + 5; // Findungsstich at index i
        }
        return null; // Findungsstich not yet found
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
}
