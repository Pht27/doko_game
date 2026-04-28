using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Rules;

namespace Doko.Domain.Tricks;

/// <summary>
/// A special rule that can override which player wins a trick.
/// Returns null when the rule does not apply.
/// </summary>
public interface ITrickWinnerRule
{
    bool IsEnabledBy(RuleSet rules);
    PlayerSeat? TryGetOverride(Trick completedTrick, GameState state, PlayerSeat normalWinner);
}
