using Doko.Domain.GameFlow;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>A trick worth ≥ 40 Augen gives the winning party +1 point.</summary>
public sealed class DoppelkopfExtrapunkt : IExtrapunkt
{
    public ExtrapunktType Type => ExtrapunktType.Doppelkopf;

    public IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state)
    {
        if (completedTrick.Points < 40) return [];

        var winner = completedTrick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
        return [new ExtrapunktAward(Type, winner, 1)];
    }
}
