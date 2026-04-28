using Doko.Domain.Cards;

namespace Doko.Domain.GameFlow;

public static class SilentModeDetector
{
    public static SilentGameMode? Detect(GameState state)
    {
        if (state.InitialHands is null)
            return null;

        var pikDame = new CardType(Suit.Pik, Rank.Dame);
        var pikKoenig = new CardType(Suit.Pik, Rank.Koenig);
        var kreuzDame = new CardType(Suit.Kreuz, Rank.Dame);

        if (state.Rules.AllowKontrasolo)
        {
            foreach (var player in state.Players)
            {
                var hand = state.InitialHands[player.Seat];
                bool hasKontrasolo =
                    hand.Cards.Count(c => c.Type == pikDame) >= 2
                    && hand.Cards.Count(c => c.Type == pikKoenig) >= 2;
                if (hasKontrasolo)
                    return new SilentGameMode(SilentGameModeType.KontraSolo, player.Seat);
            }
        }

        if (state.Rules.AllowStilleHochzeit)
        {
            foreach (var player in state.Players)
            {
                var hand = state.InitialHands[player.Seat];
                if (hand.Cards.Count(c => c.Type == kreuzDame) >= 2)
                    return new SilentGameMode(SilentGameModeType.StilleHochzeit, player.Seat);
            }
        }

        return null;
    }
}
