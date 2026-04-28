using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Rules;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Trump;

namespace Doko.Domain.Tests.Trump;

public class TrumpEvaluatorFactoryTests
{
    private static readonly RuleSet DefaultRules = RuleSet.Default();
    private static readonly TrumpEvaluatorFactory Sut = TrumpEvaluatorFactory.Instance;

    [Fact]
    public void NoActiveSonderkarten_NoReservation_ReturnsNormalEvaluator()
    {
        var result = Sut.Build(null, null, [], DefaultRules);

        result.Should().BeSameAs(NormalTrumpEvaluator.Instance);
    }

    [Fact]
    public void KontraSoloSilentMode_ReturnsKontraSoloEvaluator_WithoutDecorator()
    {
        var silentMode = new SilentGameMode(SilentGameModeType.KontraSolo, PlayerSeat.First);

        var result = Sut.Build(null, silentMode, [], DefaultRules);

        result.Should().BeSameAs(KontraSoloTrumpEvaluator.Instance);
    }

    [Fact]
    public void SoloReservation_ReturnsReservationTrumpEvaluator_WithoutDecorator()
    {
        var reservation = new DamensoloReservation(PlayerSeat.First);
        var expected = reservation.Apply().TrumpEvaluator;

        var result = Sut.Build(reservation, null, [], DefaultRules);

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public void ActiveSchweinchen_WrapsBaseInDecorator()
    {
        var result = Sut.Build(null, null, [SonderkarteType.Schweinchen], DefaultRules);

        result.Should().BeOfType<StandardSonderkarteDecorator>();
    }

    [Fact]
    public void HeidmannAndHeidfrauActive_HeidmannModifierExcluded_ReturnsPlainEvaluator()
    {
        // Heidfrau suppresses Heidmann; Heidfrau itself has no RankingModifier → no decorator
        var result = Sut.Build(
            null,
            null,
            [SonderkarteType.Heidmann, SonderkarteType.Heidfrau],
            DefaultRules
        );

        result.Should().BeSameAs(NormalTrumpEvaluator.Instance);
    }

    [Fact]
    public void ActiveGenscherdamen_NoRankingModifier_ReturnsPlainBaseEvaluator()
    {
        var result = Sut.Build(null, null, [SonderkarteType.Genscherdamen], DefaultRules);

        result.Should().BeSameAs(NormalTrumpEvaluator.Instance);
    }
}
