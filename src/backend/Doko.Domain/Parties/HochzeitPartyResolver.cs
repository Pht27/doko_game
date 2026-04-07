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
    private readonly PlayerId _hochzeitPlayer;
    private readonly HochzeitCondition _condition;

    public HochzeitPartyResolver(PlayerId hochzeitPlayer, HochzeitCondition condition)
    {
        _hochzeitPlayer = hochzeitPlayer;
        _condition = condition;
    }

    public Party? ResolveParty(PlayerId player, GameState state)
    {
        if (player == _hochzeitPlayer)
            return Party.Re;

        int qualifyingTricks = 0;
        foreach (var trick in state.CompletedTricks)
        {
            if (!Qualifies(trick, state))
                continue;
            qualifyingTricks++;

            var winner = trick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
            if (winner != _hochzeitPlayer)
                return player == winner ? Party.Re : Party.Kontra;
        }

        // After 3 qualifying tricks with no partner → Stille Hochzeit (solo)
        if (qualifyingTricks >= 3)
            return Party.Kontra;

        return null; // Still searching for partner
    }

    public bool IsFullyResolved(GameState state)
    {
        int qualifyingTricks = 0;
        foreach (var trick in state.CompletedTricks)
        {
            if (!Qualifies(trick, state))
                continue;
            qualifyingTricks++;
            var winner = trick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
            if (winner != _hochzeitPlayer)
                return true;
        }
        return qualifyingTricks >= 3;
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
