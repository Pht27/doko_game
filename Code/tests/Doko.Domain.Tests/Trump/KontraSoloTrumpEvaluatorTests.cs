namespace Doko.Domain.Tests.Trump;

public class KontraSoloTrumpEvaluatorTests
{
    private static readonly KontraSoloTrumpEvaluator Sut = KontraSoloTrumpEvaluator.Instance;

    // ── ♠ King is trump ───────────────────────────────────────────────────────

    [Fact]
    public void IsTrump_PikKoenig_ReturnsTrue() =>
        Sut.IsTrump(new CardType(Suit.Pik, Rank.Koenig)).Should().BeTrue();

    [Fact]
    public void IsTrump_NormalTrump_StillTrue() =>
        Sut.IsTrump(new CardType(Suit.Herz, Rank.Zehn)).Should().BeTrue(); // Dulle

    [Fact]
    public void IsTrump_PikAss_ReturnsFalse() =>
        Sut.IsTrump(new CardType(Suit.Pik, Rank.Ass)).Should().BeFalse();

    // ── ♠ King rank: above all Sonderkarten ──────────────────────────────────

    [Fact]
    public void GetTrumpRank_PikKoenig_IsAboveHyperschweinchen()
    {
        // Hyperschweinchen (♦ Kings elevated) reaches rank 32
        int klabautermannRank = Sut.GetTrumpRank(new CardType(Suit.Pik, Rank.Koenig));
        klabautermannRank.Should().Be(34);
    }

    [Fact]
    public void GetTrumpRank_PikKoenig_IsAboveDulle()
    {
        int klabautermannRank = Sut.GetTrumpRank(new CardType(Suit.Pik, Rank.Koenig));
        int dulleRank = Sut.GetTrumpRank(new CardType(Suit.Herz, Rank.Zehn));
        klabautermannRank.Should().BeGreaterThan(dulleRank);
    }

    [Fact]
    public void GetTrumpRank_NormalCards_Unchanged()
    {
        // Normal trump ranks should be identical to NormalTrumpEvaluator
        int kreuzDame = Sut.GetTrumpRank(new CardType(Suit.Kreuz, Rank.Dame));
        kreuzDame
            .Should()
            .Be(NormalTrumpEvaluator.Instance.GetTrumpRank(new CardType(Suit.Kreuz, Rank.Dame)));
    }

    // ── Plain ranks pass through ──────────────────────────────────────────────

    [Fact]
    public void GetPlainRank_PikAss_ReturnsNormalRank() =>
        Sut.GetPlainRank(new CardType(Suit.Pik, Rank.Ass))
            .Should()
            .Be(NormalTrumpEvaluator.Instance.GetPlainRank(new CardType(Suit.Pik, Rank.Ass)));

    [Fact]
    public void GetPlainRank_PikKoenig_Throws() =>
        FluentActions
            .Invoking(() => Sut.GetPlainRank(new CardType(Suit.Pik, Rank.Koenig)))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
}
