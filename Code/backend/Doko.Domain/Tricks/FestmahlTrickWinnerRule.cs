using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Tricks;

/// <summary>
/// If a trick contains ≥ 3 animals and at least two are the same type, the trick winner is
/// overridden: the second card of the majority animal type (in play order) wins.
/// If there are exactly two pairs, the last card of the trick wins instead.
/// Blutbad takes precedence (≥ 3 distinct kinds); Festmahl only applies when &lt; 3 distinct kinds.
/// </summary>
public sealed class FestmahlTrickWinnerRule : ITrickWinnerRule
{
    public PlayerSeat? TryGetOverride(
        Trick completedTrick,
        GameState state,
        PlayerSeat normalWinner
    )
    {
        var animals = AnimalHelpers.GetAnimals(completedTrick, state);
        if (animals.Count < 3)
            return null;

        // Blutbad takes precedence when ≥ 3 different animal types are present
        int distinctKinds = animals.Select(a => a.Kind).Distinct().Count();
        if (distinctKinds >= 3)
            return null;

        var byKind = animals.GroupBy(a => a.Kind).OrderByDescending(g => g.Count()).ToList();
        if (byKind[0].Count() < 2)
            return null;

        int majorityCount = byKind[0].Count();
        bool twoPairs =
            byKind.Count >= 2 && byKind[1].Count() == majorityCount && majorityCount == 2;

        if (twoPairs)
            return completedTrick.Cards[^1].Player;

        return byKind[0].ElementAt(1).Card.Player;
    }
}
