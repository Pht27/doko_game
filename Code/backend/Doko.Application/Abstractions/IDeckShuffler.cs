using Doko.Domain.Cards;

namespace Doko.Application.Abstractions;

public interface IDeckShuffler
{
    IReadOnlyList<Card> Shuffle(IReadOnlyList<Card> deck);
}
