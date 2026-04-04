using Doko.Domain.Tests.Helpers;

namespace Doko.Domain.Tests.Hands;

public class HandTests
{
    private static Card KreuzAss  => B.Card(0, Suit.Kreuz, Rank.Ass);
    private static Card KreuzAss2 => B.Card(1, Suit.Kreuz, Rank.Ass); // same type, different Id
    private static Card PikKoenig => B.Card(2, Suit.Pik,   Rank.Koenig);

    [Fact]
    public void Remove_ReturnsNewHandWithoutCard()
    {
        var hand = B.HandOf(KreuzAss, PikKoenig);
        var result = hand.Remove(KreuzAss);
        result.Cards.Should().ContainSingle().Which.Should().Be(PikKoenig);
    }

    [Fact]
    public void Remove_IsNonDestructive_OriginalUnchanged()
    {
        var hand = B.HandOf(KreuzAss, PikKoenig);
        _ = hand.Remove(KreuzAss);
        hand.Cards.Should().HaveCount(2);
    }

    [Fact]
    public void Remove_RemovesOnlyOneCard_WhenSameTypeExistsTwice()
    {
        var hand = B.HandOf(KreuzAss, KreuzAss2); // two ♣A with different Ids
        var result = hand.Remove(KreuzAss);
        result.Cards.Should().ContainSingle().Which.Id.Should().Be(KreuzAss2.Id);
    }

    [Fact]
    public void Remove_ThrowsInvalidOperationException_WhenCardNotPresent()
    {
        var hand = B.HandOf(PikKoenig);
        var act = () => hand.Remove(KreuzAss);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Remove_MatchesByCardId_NotOnlyByType()
    {
        // KreuzAss2 has a different Id from KreuzAss; removing KreuzAss should leave KreuzAss2
        var hand = B.HandOf(KreuzAss, KreuzAss2);
        var result = hand.Remove(KreuzAss);
        result.Contains(KreuzAss).Should().BeFalse();
        result.Contains(KreuzAss2).Should().BeTrue();
    }
}
