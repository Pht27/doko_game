using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>A trick worth ≥ 40 Augen gives the winning party +1 point.</summary>
public sealed class DoppelkopfExtrapunkt : IExtrapunkt
{
    public ExtrapunktType Type => ExtrapunktType.Doppelkopf;

    public IReadOnlyList<ExtrapunktAward> Evaluate(
        Trick completedTrick,
        GameState state,
        PlayerSeat effectiveTrickWinner
    )
    {
        if (completedTrick.Points < 40)
            return [];

        return [new ExtrapunktAward(Type, effectiveTrickWinner, 1)];
    }
}
