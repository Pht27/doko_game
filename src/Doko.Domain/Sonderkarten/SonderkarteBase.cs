using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Trump;

namespace Doko.Domain.Sonderkarten;

public abstract class SonderkarteBase : ISonderkarte
{
    public abstract SonderkarteType Type { get; }
    public abstract CardType TriggeringCard { get; }
    public virtual ISonderkarteRankingModifier? RankingModifier => null;
    public virtual SonderkarteType? Suppresses => null;
    public virtual bool WindowClosesWhenDeclined => true;
    public abstract bool AreConditionsMet(GameState state);
    public abstract GameStateModification? Apply(GameState state);

    /// <summary>
    /// Returns true if the current player's initial hand contained at least two copies of
    /// <paramref name="cardType"/>. Call before the triggering card is removed from the hand.
    /// </summary>
    protected static bool OriginallyHeldBoth(GameState state, CardType cardType)
        => state.InitialHands is not null
        && state.InitialHands[state.CurrentTurn].Cards.Count(c => c.Type == cardType) >= 2;

    protected static bool IsActive(GameState state, SonderkarteType type)
        => state.ActiveSonderkarten.Contains(type);

    protected static bool IsWindowClosed(GameState state, SonderkarteType type)
        => state.ClosedWindows.Contains(type);

    /// <summary>
    /// Returns true if any single player played both copies of <paramref name="cardType"/>
    /// across all completed tricks and the current trick. Used to allow chain activations
    /// (e.g. Superschweinchen) even when the prerequisite sonderkarte was never announced.
    /// </summary>
    protected static bool BothPlayedBySamePlayer(GameState state, CardType cardType)
    {
        var played = state.CompletedTricks
            .SelectMany(t => t.Cards)
            .Concat(state.CurrentTrick?.Cards ?? [])
            .Where(tc => tc.Card.Type == cardType)
            .ToList();
        return played.Count >= 2
            && played.GroupBy(tc => tc.Player).Any(g => g.Count() >= 2);
    }
}
