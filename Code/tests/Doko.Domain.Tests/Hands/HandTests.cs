using Doko.Domain.Tests.Helpers;
using Doko.Domain.Trump;

namespace Doko.Domain.Tests.Hands;

public class HandTests
{
    private static Card KreuzAss => B.Card(0, Suit.Kreuz, Rank.Ass);
    private static Card KreuzAss2 => B.Card(1, Suit.Kreuz, Rank.Ass); // same type, different Id
    private static Card PikKoenig => B.Card(2, Suit.Pik, Rank.Koenig);

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

    // ── SortedFor ─────────────────────────────────────────────────────────────

    private static readonly ITrumpEvaluator Trump = NormalTrumpEvaluator.Instance;

    [Fact]
    public void SortedFor_TrumpCardsSortBeforeNonTrumpCards()
    {
        var trump = B.Card(0, Suit.Karo, Rank.Ass); // Karo-Ass is trump
        var fehl = B.Card(1, Suit.Kreuz, Rank.Ass); // Kreuz-Ass is Fehl
        var hand = B.HandOf(fehl, trump);

        var sorted = hand.SortedFor(Trump);

        sorted[0].Should().Be(trump);
        sorted[1].Should().Be(fehl);
    }

    [Fact]
    public void SortedFor_HigherTrumpRankSortsFirstWithinTrumps()
    {
        var kreuzDame = B.Card(0, Suit.Kreuz, Rank.Dame); // rank 24
        var karoBube = B.Card(1, Suit.Karo, Rank.Bube); // rank 10
        var hand = B.HandOf(karoBube, kreuzDame);

        var sorted = hand.SortedFor(Trump);

        sorted[0].Should().Be(kreuzDame);
        sorted[1].Should().Be(karoBube);
    }

    [Fact]
    public void SortedFor_FehlCardsGroupBySuitInSuitOrder()
    {
        var kreuzAss = B.Card(0, Suit.Kreuz, Rank.Ass);
        var pikAss = B.Card(1, Suit.Pik, Rank.Ass);
        var herzAss = B.Card(2, Suit.Herz, Rank.Ass);
        var hand = B.HandOf(herzAss, pikAss, kreuzAss);

        var sorted = hand.SortedFor(Trump);

        sorted.Select(c => c.Type.Suit).Should().Equal(Suit.Kreuz, Suit.Pik, Suit.Herz);
    }

    [Fact]
    public void SortedFor_HigherPlainRankSortsFirstWithinSameSuit()
    {
        var kreuzAss = B.Card(0, Suit.Kreuz, Rank.Ass); // plain rank 4
        var kreuzNeun = B.Card(1, Suit.Kreuz, Rank.Neun); // plain rank 1
        var kreuzZehn = B.Card(2, Suit.Kreuz, Rank.Zehn); // plain rank 3
        var hand = B.HandOf(kreuzNeun, kreuzAss, kreuzZehn);

        var sorted = hand.SortedFor(Trump);

        sorted.Select(c => c.Type.Rank).Should().Equal(Rank.Ass, Rank.Zehn, Rank.Neun);
    }
}
