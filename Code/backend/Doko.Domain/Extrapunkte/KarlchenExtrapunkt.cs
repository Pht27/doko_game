using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
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

    public IReadOnlyList<ExtrapunktAward> Evaluate(
        Trick completedTrick,
        GameState state,
        PlayerSeat effectiveTrickWinner
    )
    {
        bool heidmannActive = state.ActiveSonderkarten.Contains(SonderkarteType.Heidmann);
        bool heidfrauActive = state.ActiveSonderkarten.Contains(SonderkarteType.Heidfrau);
        if (heidmannActive && !heidfrauActive)
            return [];

        if (state.CompletedTricks.Count != state.Rules.LastTrickIndex)
            return [];

        bool winnerPlayedKarlchen = completedTrick.Cards.Any(tc =>
            tc.Card.Type == KreuzBube && tc.Player == effectiveTrickWinner
        );

        return winnerPlayedKarlchen ? [new ExtrapunktAward(Type, effectiveTrickWinner, 1)] : [];
    }
}
