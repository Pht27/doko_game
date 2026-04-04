using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>A trick consisting of 4 Queens (any suits) gives the winning party +1 point.</summary>
public sealed class KaffeekranzExtrapunkt : IExtrapunkt
{
    public ExtrapunktType Type => ExtrapunktType.Kaffeekranzchen;

    public IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state)
    {
        if (!completedTrick.Cards.All(tc => tc.Card.Type.Rank == Rank.Dame)) return [];

        var winner = completedTrick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
        return [new ExtrapunktAward(Type, winner, 1)];
    }
}
