using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Modifications;
using Doko.Domain.Trump;

namespace Doko.Domain.Sonderkarten;

public interface ISonderkarte
{
    SonderkarteType Type { get; }

    /// <summary>
    /// The card type whose play triggers the eligibility check.
    /// The check runs when this card is played, before the card is removed from the player's hand.
    /// </summary>
    CardType TriggeringCard { get; }

    /// <summary>
    /// The trump ranking modifier introduced when this sonderkarte becomes active, or null if it
    /// has no effect on trump order. Consumed by <see cref="GameState"/> when rebuilding the evaluator.
    /// </summary>
    ISonderkarteRankingModifier? RankingModifier { get; }

    /// <summary>
    /// When non-null, activating this sonderkarte removes the modifier of the named type from the
    /// evaluator (e.g. Heidfrau suppresses Heidmann). Null for sonderkarten with no suppression.
    /// </summary>
    SonderkarteType? Suppresses { get; }

    /// <summary>
    /// Returns true if the player may claim this sonderkarte right now.
    /// Called before the triggering card is removed from <see cref="GameState.Players"/>,
    /// so the current hand still contains it. Use <see cref="GameState.InitialHands"/> to check
    /// original holding (e.g. Superschweinchen: originally held both ♦10 even if first already played).
    /// </summary>
    bool AreConditionsMet(GameState state);

    IReadOnlyList<GameStateModification> Apply(GameState state, ISonderkarteInputProvider inputs);

    /// <summary>
    /// When true, the activation window for this sonderkarte closes permanently the moment
    /// the player plays the triggering card without activating it.
    /// When false (e.g. LinksGehangter, Kemmerich), the player may decide on any qualifying play.
    /// </summary>
    bool WindowClosesWhenDeclined { get; }
}
