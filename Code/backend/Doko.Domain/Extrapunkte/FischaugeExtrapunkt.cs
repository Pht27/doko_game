using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
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

    public IReadOnlyList<ExtrapunktAward> Evaluate(
        Trick completedTrick,
        GameState state,
        PlayerSeat effectiveTrickWinner
    )
    {
        if (!AnimalHelpers.FischaugeActive(state))
            return [];

        bool winnerPlayedFischauge = completedTrick.Cards.Any(tc =>
            tc.Card.Type == KaroNeun && tc.Player == effectiveTrickWinner
        );

        return winnerPlayedFischauge ? [new ExtrapunktAward(Type, effectiveTrickWinner, 1)] : [];
    }
}
