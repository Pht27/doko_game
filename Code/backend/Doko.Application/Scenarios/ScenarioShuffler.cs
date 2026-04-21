using Doko.Application.Abstractions;
using Doko.Domain.Cards;

namespace Doko.Application.Scenarios;

public sealed class ScenarioShuffler(ScenarioConfig config) : IDeckShuffler
{
    public IReadOnlyList<Card> Shuffle(IReadOnlyList<Card> deck)
    {
        var remaining = deck.ToList();
        for (int i = remaining.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (remaining[i], remaining[j]) = (remaining[j], remaining[i]);
        }
        var slotSize = deck.Count / 4;
        var result = new List<Card>(deck.Count);

        for (int player = 0; player < 4; player++)
        {
            var slot = new List<Card>(slotSize);

            if (config.PlayerRequiredCards.TryGetValue(player, out var required))
            {
                foreach (var cardType in required)
                {
                    var idx = remaining.FindIndex(c => c.Type == cardType);
                    slot.Add(remaining[idx]);
                    remaining.RemoveAt(idx);
                }
            }

            int toFill = slotSize - slot.Count;
            slot.AddRange(remaining.Take(toFill));
            remaining.RemoveRange(0, toFill);

            result.AddRange(slot);
        }

        return result;
    }
}
