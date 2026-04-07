using Doko.Application.Abstractions;
using Doko.Domain.Cards;

namespace Doko.Infrastructure.Shuffler;

public sealed class RandomDeckShuffler : IDeckShuffler
{
    public IReadOnlyList<Card> Shuffle(IReadOnlyList<Card> deck)
    {
        var list = deck.ToList();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}
