using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>
/// After the first trump card is played, ♦ Nines become Fischaugen.
/// If a Fischauge wins a trick, the winning party gets +1 point.
/// </summary>
public sealed class FischaugeExtrapunkt : IExtrapunkt
{
    private static readonly CardType KaroNeun = new(Suit.Karo, Rank.Neun);

    public ExtrapunktType Type => ExtrapunktType.Fischauge;

    public IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state)
    {
        if (!AnimalHelpers.FischaugeActive(state)) return [];

        var winner = completedTrick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
        bool winnerPlayedFischauge = completedTrick.Cards
            .Any(tc => tc.Card.Type == KaroNeun && tc.Player == winner);

        return winnerPlayedFischauge ? [new ExtrapunktAward(Type, winner, 1)] : [];
    }
}
