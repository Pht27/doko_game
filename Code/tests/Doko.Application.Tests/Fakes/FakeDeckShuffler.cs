using Doko.Application.Abstractions;
using Doko.Domain.Cards;

namespace Doko.Application.Tests.Fakes;

/// <summary>Returns the deck in a fixed, deterministic order (no shuffle).</summary>
public sealed class FakeDeckShuffler : IDeckShuffler
{
    public IReadOnlyList<Card> Shuffle(IReadOnlyList<Card> deck) => deck;
}
