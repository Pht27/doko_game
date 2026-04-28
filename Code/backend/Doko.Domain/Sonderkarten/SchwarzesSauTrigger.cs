using Doko.Domain.Cards;
using Doko.Domain.GameFlow;

namespace Doko.Domain.Sonderkarten;

public static class SchwarzesSauTrigger
{
    /// <summary>
    /// Returns true when the just-completed trick pushed the running ♠Q count from &lt;2 to ≥2.
    /// Handles the case where both Pik Damen appear in the same trick.
    /// </summary>
    public static bool IsSecondPikDameTrick(GameState state)
    {
        var pikDame = new CardType(Suit.Pik, Rank.Dame);
        var justCompleted = state.CompletedTricks.Last();

        int inThisTrick = justCompleted.Cards.Count(c => c.Card.Type == pikDame);
        if (inThisTrick == 0)
            return false;

        int totalSoFar = state.CompletedTricks.Sum(t => t.Cards.Count(c => c.Card.Type == pikDame));
        int beforeThisTrick = totalSoFar - inThisTrick;

        return beforeThisTrick < 2 && totalSoFar >= 2;
    }
}
