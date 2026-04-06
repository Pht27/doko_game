namespace Doko.Domain.Tests.Trump;

public class SonderkarteDecoratorTests
{
    private static readonly CardType KaroAss = new(Suit.Karo, Rank.Ass);
    private static readonly CardType KaroZehn = new(Suit.Karo, Rank.Zehn);
    private static readonly CardType KaroKoenig = new(Suit.Karo, Rank.Koenig);
    private static readonly CardType Dulle = new(Suit.Herz, Rank.Zehn);
    private static readonly CardType KreuzDame = new(Suit.Kreuz, Rank.Dame);
    private static readonly CardType KreuzBube = new(Suit.Kreuz, Rank.Bube);
    private static readonly CardType PikBube = new(Suit.Pik, Rank.Bube);

    // ── Schweinchen ──────────────────────────────────────────────────────────

    [Fact]
    public void Schweinchen_ElevatesKaroAssAboveDulle()
    {
        var sut = Decorated(SchweinchenModifier.Instance);
        sut.GetTrumpRank(KaroAss).Should().BeGreaterThan(sut.GetTrumpRank(Dulle));
    }

    [Fact]
    public void Schweinchen_DoesNotAffectOtherCards()
    {
        var plain = NormalTrumpEvaluator.Instance;
        var sut = Decorated(SchweinchenModifier.Instance);
        sut.GetTrumpRank(KreuzDame).Should().Be(plain.GetTrumpRank(KreuzDame));
    }

    // ── Superschweinchen ────────────────────────────────────────────────────

    [Fact]
    public void Superschweinchen_ElevatesKaroZehnAboveSchweinchen()
    {
        var sut = Decorated(SchweinchenModifier.Instance, SuperschweinchenModifier.Instance);
        sut.GetTrumpRank(KaroZehn).Should().BeGreaterThan(sut.GetTrumpRank(KaroAss));
    }

    // ── Hyperschweinchen ─────────────────────────────────────────────────────

    [Fact]
    public void Hyperschweinchen_ElevatesKaroKoenigAboveSuperschweinchen()
    {
        var sut = Decorated(
            SchweinchenModifier.Instance,
            SuperschweinchenModifier.Instance,
            HyperschweinchenModifier.Instance
        );
        sut.GetTrumpRank(KaroKoenig).Should().BeGreaterThan(sut.GetTrumpRank(KaroZehn));
    }

    // ── Heidmann ─────────────────────────────────────────────────────────────

    [Fact]
    public void Heidmann_JacksRankHigherThanQueens()
    {
        var sut = Decorated(HeidmannModifier.Instance);
        sut.GetTrumpRank(KreuzBube).Should().BeGreaterThan(sut.GetTrumpRank(KreuzDame));
    }

    [Fact]
    public void Heidmann_PikBubeBeatsKreuzDame()
    {
        var sut = Decorated(HeidmannModifier.Instance);
        sut.GetTrumpRank(PikBube).Should().BeGreaterThan(sut.GetTrumpRank(KreuzDame));
    }

    // ── Heidfrau (suppresses Heidmann via GameState) ─────────────────────────

    [Fact]
    public void WhenHeidfrauActive_QueensRankAboveJacks_RestoredToNormal()
    {
        // Heidfrau suppresses the Heidmann modifier — evaluated via GameState.
        // Simulate by using the decorator WITHOUT Heidmann modifier.
        var plain = NormalTrumpEvaluator.Instance;
        var withoutHeidmann = new StandardSonderkarteDecorator(plain, []);
        withoutHeidmann
            .GetTrumpRank(KreuzDame)
            .Should()
            .BeGreaterThan(withoutHeidmann.GetTrumpRank(KreuzBube));
    }

    // ── IsTrump passthrough ───────────────────────────────────────────────────

    [Fact]
    public void Decorator_IsTrump_DelegatesToInner()
    {
        var sut = Decorated(SchweinchenModifier.Instance);
        sut.IsTrump(new CardType(Suit.Kreuz, Rank.Ass)).Should().BeFalse(); // plain
        sut.IsTrump(KaroAss).Should().BeTrue();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static StandardSonderkarteDecorator Decorated(
        params ISonderkarteRankingModifier[] modifiers
    ) => new(NormalTrumpEvaluator.Instance, modifiers);
}
