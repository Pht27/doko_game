using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Rules;

namespace Doko.Domain.Tricks;

/// <summary>
/// If a trick contains ≥ 3 different animal types, the trick winner is overridden:
/// the non-animal card wins. If all cards are animals, the Fischauge wins.
/// Blutbad takes precedence over Festmahl.
/// </summary>
public sealed class BlutbadTrickWinnerRule : ITrickWinnerRule
{
    public bool IsEnabledBy(RuleSet rules) => rules.EnableBlutbad;

    public PlayerSeat? TryGetOverride(
        Trick completedTrick,
        PlayingState state,
        PlayerSeat normalWinner
    )
    {
        var animals = AnimalHelpers.GetAnimals(completedTrick, state);
        if (animals.Count < 3)
            return null;

        int distinctKinds = animals.Select(a => a.Kind).Distinct().Count();
        if (distinctKinds < 3)
            return null;

        var animalCards = animals.Select(a => a.Card).ToHashSet();
        var nonAnimals = completedTrick.Cards.Where(tc => !animalCards.Contains(tc)).ToList();

        if (nonAnimals.Count == 0)
        {
            // All cards are animals → Fischauge wins
            return animals
                .Where(a => a.Kind == AnimalKind.Fischauge)
                .Select(a => (PlayerSeat?)a.Card.Player)
                .FirstOrDefault();
        }

        return nonAnimals[0].Player;
    }
}
