namespace Doko.Domain.Tests.Cards;

public class DeckTests
{
    [Fact]
    public void Standard48_HasCorrectCount() => Deck.Standard48().Should().HaveCount(48);

    [Fact]
    public void Standard40_HasCorrectCount() => Deck.Standard40().Should().HaveCount(40);

    [Fact]
    public void Standard48_AllCardIdsAreUnique()
    {
        var ids = Deck.Standard48().Select(c => c.Id.Value).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Standard48_EachCardTypeAppearsTwice()
    {
        var groups = Deck.Standard48().GroupBy(c => c.Type).ToList();
        groups.Should().HaveCount(24);
        groups.Should().AllSatisfy(g => g.Should().HaveCount(2));
    }

    [Fact]
    public void Standard40_EachCardTypeAppearsTwice()
    {
        var groups = Deck.Standard40().GroupBy(c => c.Type).ToList();
        groups.Should().HaveCount(20);
        groups.Should().AllSatisfy(g => g.Should().HaveCount(2));
    }

    [Fact]
    public void Standard48_ContainsAllNines()
    {
        var nines = Deck.Standard48().Where(c => c.Type.Rank == Rank.Neun).ToList();
        nines.Should().HaveCount(8); // 4 suits × 2 copies
    }

    [Fact]
    public void Standard40_ContainsNoNines() =>
        Deck.Standard40().Should().NotContain(c => c.Type.Rank == Rank.Neun);

    [Fact]
    public void Standard48_AugenSumTo240() =>
        Deck.Standard48().Sum(c => CardPoints.Of(c.Type.Rank)).Should().Be(240);

    [Fact]
    public void Standard40_AugenSumTo240() =>
        Deck.Standard40().Sum(c => CardPoints.Of(c.Type.Rank)).Should().Be(240);
}
