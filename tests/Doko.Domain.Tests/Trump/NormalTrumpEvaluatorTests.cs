namespace Doko.Domain.Tests.Trump;

public class NormalTrumpEvaluatorTests
{
    private static readonly NormalTrumpEvaluator Sut = NormalTrumpEvaluator.Instance;

    // ── IsTrump ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(Suit.Kreuz, Rank.Dame)]
    [InlineData(Suit.Pik, Rank.Dame)]
    [InlineData(Suit.Herz, Rank.Dame)]
    [InlineData(Suit.Karo, Rank.Dame)]
    [InlineData(Suit.Kreuz, Rank.Bube)]
    [InlineData(Suit.Pik, Rank.Bube)]
    [InlineData(Suit.Herz, Rank.Bube)]
    [InlineData(Suit.Karo, Rank.Bube)]
    [InlineData(Suit.Karo, Rank.Ass)]
    [InlineData(Suit.Karo, Rank.Koenig)]
    [InlineData(Suit.Karo, Rank.Zehn)]
    [InlineData(Suit.Karo, Rank.Neun)]
    [InlineData(Suit.Herz, Rank.Zehn)] // Dulle
    public void IsTrump_ReturnsTrue(Suit suit, Rank rank) =>
        Sut.IsTrump(new CardType(suit, rank)).Should().BeTrue();

    [Theory]
    [InlineData(Suit.Kreuz, Rank.Ass)]
    [InlineData(Suit.Kreuz, Rank.Zehn)]
    [InlineData(Suit.Kreuz, Rank.Koenig)]
    [InlineData(Suit.Kreuz, Rank.Neun)]
    [InlineData(Suit.Pik, Rank.Ass)]
    [InlineData(Suit.Pik, Rank.Koenig)]
    [InlineData(Suit.Herz, Rank.Ass)]
    [InlineData(Suit.Herz, Rank.Koenig)]
    [InlineData(Suit.Herz, Rank.Neun)]
    public void IsTrump_ReturnsFalse_ForPlainCards(Suit suit, Rank rank) =>
        Sut.IsTrump(new CardType(suit, rank)).Should().BeFalse();

    // ── Trump rank ordering ───────────────────────────────────────────────────

    [Fact]
    public void GetTrumpRank_Dulle_IsHighestStandardTrump()
    {
        int dulleRank = Sut.GetTrumpRank(new CardType(Suit.Herz, Rank.Zehn));
        int kreuzDameRank = Sut.GetTrumpRank(new CardType(Suit.Kreuz, Rank.Dame));
        dulleRank.Should().BeGreaterThan(kreuzDameRank);
    }

    [Fact]
    public void GetTrumpRank_DamesRankInSuitOrder()
    {
        int kreuz = Sut.GetTrumpRank(new CardType(Suit.Kreuz, Rank.Dame));
        int pik = Sut.GetTrumpRank(new CardType(Suit.Pik, Rank.Dame));
        int herz = Sut.GetTrumpRank(new CardType(Suit.Herz, Rank.Dame));
        int karo = Sut.GetTrumpRank(new CardType(Suit.Karo, Rank.Dame));
        kreuz.Should().BeGreaterThan(pik);
        pik.Should().BeGreaterThan(herz);
        herz.Should().BeGreaterThan(karo);
    }

    [Fact]
    public void GetTrumpRank_KreuzBubeBeatsAllOtherJacks()
    {
        int kreuz = Sut.GetTrumpRank(new CardType(Suit.Kreuz, Rank.Bube));
        int pik = Sut.GetTrumpRank(new CardType(Suit.Pik, Rank.Bube));
        int herz = Sut.GetTrumpRank(new CardType(Suit.Herz, Rank.Bube));
        int karo = Sut.GetTrumpRank(new CardType(Suit.Karo, Rank.Bube));
        kreuz.Should().BeGreaterThan(pik).And.BeGreaterThan(herz).And.BeGreaterThan(karo);
    }

    [Fact]
    public void GetTrumpRank_KaroSuitOrder()
    {
        int ass = Sut.GetTrumpRank(new CardType(Suit.Karo, Rank.Ass));
        int koenig = Sut.GetTrumpRank(new CardType(Suit.Karo, Rank.Koenig));
        int zehn = Sut.GetTrumpRank(new CardType(Suit.Karo, Rank.Zehn));
        int neun = Sut.GetTrumpRank(new CardType(Suit.Karo, Rank.Neun));
        ass.Should().BeGreaterThan(koenig).And.BeGreaterThan(zehn).And.BeGreaterThan(neun);
        koenig.Should().BeGreaterThan(zehn).And.BeGreaterThan(neun);
        zehn.Should().BeGreaterThan(neun);
    }

    // ── Plain rank ordering ───────────────────────────────────────────────────

    [Theory]
    [InlineData(Suit.Kreuz)]
    [InlineData(Suit.Pik)]
    public void GetPlainRank_KreuzAndPik_AssBeatsZehnBeatsKoenig(Suit suit)
    {
        int ass = Sut.GetPlainRank(new CardType(suit, Rank.Ass));
        int zehn = Sut.GetPlainRank(new CardType(suit, Rank.Zehn));
        int koenig = Sut.GetPlainRank(new CardType(suit, Rank.Koenig));
        ass.Should().BeGreaterThan(zehn);
        zehn.Should().BeGreaterThan(koenig);
    }

    [Fact]
    public void GetPlainRank_Herz_NoZehn_AssBeatsKoenig()
    {
        // ♥10 is Dulle (trump), so Herz has no plain Zehn
        int ass = Sut.GetPlainRank(new CardType(Suit.Herz, Rank.Ass));
        int koenig = Sut.GetPlainRank(new CardType(Suit.Herz, Rank.Koenig));
        ass.Should().BeGreaterThan(koenig);
    }
}
