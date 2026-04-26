using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Tricks;

/// <summary>
/// A special rule that can override which player wins a trick.
/// Returns null when the rule does not apply.
/// </summary>
public interface ITrickWinnerRule
{
    PlayerSeat? TryGetOverride(Trick completedTrick, GameState state, PlayerSeat normalWinner);
}
