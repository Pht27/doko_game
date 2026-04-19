using Doko.Domain.GameFlow;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>
/// If a trick contains ≥ 3 different animal types, the trick winner is overridden:
/// the non-animal card wins. If all cards are animals, the Fischauge wins.
/// Blutbad takes precedence over Festmahl.
/// The award's BenefittingPlayer is the Blutbad winner (may differ from the normal trick winner).
/// </summary>
public sealed class BlutbadExtrapunkt : IExtrapunkt
{
    public ExtrapunktType Type => ExtrapunktType.Blutbad;

    public IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state)
    {
        var animals = AnimalHelpers.GetAnimals(completedTrick, state);
        if (animals.Count < 3)
            return [];

        int distinctKinds = animals.Select(a => a.Kind).Distinct().Count();
        if (distinctKinds < 3)
            return [];

        Players.PlayerSeat blutbadWinner;
        var animalCards = animals.Select(a => a.Card).ToHashSet();
        var nonAnimals = completedTrick.Cards.Where(tc => !animalCards.Contains(tc)).ToList();

        if (nonAnimals.Count == 0)
        {
            // All cards are animals → Fischauge wins (first Fischauge in play order)
            var fischauge = animals.First(a => a.Kind == AnimalKind.Fischauge);
            blutbadWinner = fischauge.Card.Player;
        }
        else
        {
            // Non-animal wins (with ≥ 3 animals there is at most one non-animal)
            blutbadWinner = nonAnimals[0].Player;
        }

        // Delta = 0: Blutbad does not score a bonus point; the BenefittingPlayer is
        // the actual trick winner under Blutbad rules (game engine uses this to override).
        return [new ExtrapunktAward(Type, blutbadWinner, 0)];
    }
}
