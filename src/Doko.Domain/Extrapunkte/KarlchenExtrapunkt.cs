using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>
/// A ♣ Jack (Karlchen) winning the last trick gives the winning party +1 point.
/// Deactivated while Heidmann is in effect (reactivated if Heidfrau reverts it).
/// </summary>
public sealed class KarlchenExtrapunkt : IExtrapunkt
{
    private static readonly CardType KreuzBube = new(Suit.Kreuz, Rank.Bube);

    public ExtrapunktType Type => ExtrapunktType.Karlchen;

    public IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state)
    {
        bool heidmannActive = state.ActiveSonderkarten.Contains(SonderkarteType.Heidmann);
        bool heidfrauActive = state.ActiveSonderkarten.Contains(SonderkarteType.Heidfrau);
        if (heidmannActive && !heidfrauActive) return [];

        // Only on the last trick (12 tricks total; CompletedTricks has 11 before this one is added)
        if (state.CompletedTricks.Count != 11) return [];

        var winner = completedTrick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
        bool winnerPlayedKarlchen = completedTrick.Cards
            .Any(tc => tc.Card.Type == KreuzBube && tc.Player == winner);

        return winnerPlayedKarlchen ? [new ExtrapunktAward(Type, winner, 1)] : [];
    }
}
