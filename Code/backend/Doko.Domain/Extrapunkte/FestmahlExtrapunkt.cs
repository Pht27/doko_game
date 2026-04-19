using Doko.Domain.GameFlow;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>
/// If a trick contains ≥ 3 animals and at least two are the same type, the trick winner is
/// overridden: the second card of the majority animal type (in play order) wins.
/// If there are exactly two pairs, the last card of the trick wins instead.
/// Blutbad takes precedence; returns empty if Blutbad conditions are also met.
/// The award's BenefittingPlayer is the Festmahl winner (may differ from the normal trick winner).
/// </summary>
public sealed class FestmahlExtrapunkt : IExtrapunkt
{
    public ExtrapunktType Type => ExtrapunktType.Festmahl;

    public IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state)
    {
        var animals = AnimalHelpers.GetAnimals(completedTrick, state);
        if (animals.Count < 3)
            return [];

        // Blutbad takes precedence when ≥ 3 different animal types are present
        int distinctKinds = animals.Select(a => a.Kind).Distinct().Count();
        if (distinctKinds >= 3)
            return [];

        // Need at least two of the same type
        var byKind = animals.GroupBy(a => a.Kind).OrderByDescending(g => g.Count()).ToList();

        if (byKind[0].Count() < 2)
            return [];

        Players.PlayerSeat festmahlWinner;
        int majorityCount = byKind[0].Count();

        bool twoPairs =
            byKind.Count >= 2 && byKind[1].Count() == majorityCount && majorityCount == 2;

        if (twoPairs)
        {
            // Last card of the trick wins
            festmahlWinner = completedTrick.Cards[^1].Player;
        }
        else
        {
            // Second card of majority type (in play order) wins
            festmahlWinner = byKind[0].ElementAt(1).Card.Player;
        }

        // Delta = 0: Festmahl does not score a bonus point; the BenefittingPlayer is
        // the actual trick winner under Festmahl rules (game engine uses this to override).
        return [new ExtrapunktAward(Type, festmahlWinner, 0)];
    }
}
