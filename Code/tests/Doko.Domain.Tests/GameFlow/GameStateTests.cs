using Doko.Domain.Tests.Helpers;

namespace Doko.Domain.Tests.GameFlow;

public class GameStateTests
{
    // ── NextPlayer ────────────────────────────────────────────────────────────

    [Fact]
    public void NextPlayer_Counterclockwise_AdvancesBySeat()
    {
        var state = B.BasicState();
        state.NextPlayer(B.P0, PlayDirection.Counterclockwise).Should().Be(B.P1);
        state.NextPlayer(B.P1, PlayDirection.Counterclockwise).Should().Be(B.P2);
        state.NextPlayer(B.P2, PlayDirection.Counterclockwise).Should().Be(B.P3);
    }

    [Fact]
    public void NextPlayer_Counterclockwise_WrapsFromLastToFirst()
    {
        var state = B.BasicState();
        state.NextPlayer(B.P3, PlayDirection.Counterclockwise).Should().Be(B.P0);
    }

    [Fact]
    public void NextPlayer_Clockwise_DecreasesSeat()
    {
        var state = B.BasicState();
        state.NextPlayer(B.P3, PlayDirection.Clockwise).Should().Be(B.P2);
        state.NextPlayer(B.P2, PlayDirection.Clockwise).Should().Be(B.P1);
        state.NextPlayer(B.P1, PlayDirection.Clockwise).Should().Be(B.P0);
    }

    [Fact]
    public void NextPlayer_Clockwise_WrapsFromFirstToLast()
    {
        var state = B.BasicState();
        state.NextPlayer(B.P0, PlayDirection.Clockwise).Should().Be(B.P3);
    }

    // ── Apply: ReverseDirectionModification ───────────────────────────────────

    [Fact]
    public void Apply_ReverseDirection_FlipsCounterclockwiseToClockwise()
    {
        var state = GameState.Create(direction: PlayDirection.Counterclockwise);
        state.Apply(new ReverseDirectionModification());
        state.Direction.Should().Be(PlayDirection.Clockwise);
    }

    [Fact]
    public void Apply_ReverseDirection_FlipsClockwiseToCounterclockwise()
    {
        var state = GameState.Create(direction: PlayDirection.Clockwise);
        state.Apply(new ReverseDirectionModification());
        state.Direction.Should().Be(PlayDirection.Counterclockwise);
    }

    [Fact]
    public void Apply_ReverseDirection_TwiceRestoresOriginal()
    {
        var state = GameState.Create(direction: PlayDirection.Counterclockwise);
        state.Apply(new ReverseDirectionModification());
        state.Apply(new ReverseDirectionModification());
        state.Direction.Should().Be(PlayDirection.Counterclockwise);
    }

    // ── Apply: WithdrawAnnouncementModification ───────────────────────────────

    [Fact]
    public void Apply_WithdrawAnnouncement_RemovesMatchingAnnouncement()
    {
        var ann = B.Ann(B.P0, AnnouncementType.Win);
        var state = GameState.Create(announcements: [ann]);

        state.Apply(new WithdrawAnnouncementModification(B.P0, AnnouncementType.Win));

        state.Announcements.Should().BeEmpty();
    }

    [Fact]
    public void Apply_WithdrawAnnouncement_LeavesOtherAnnouncementsIntact()
    {
        var re = B.Ann(B.P0, AnnouncementType.Win);
        var kontra = B.Ann(B.P1, AnnouncementType.Win);
        var state = GameState.Create(announcements: [re, kontra]);

        state.Apply(new WithdrawAnnouncementModification(B.P0, AnnouncementType.Win));

        state.Announcements.Should().ContainSingle().Which.Should().Be(kontra);
    }


    // ── Apply: ActivateSonderkarteModification ────────────────────────────────

    [Fact]
    public void Apply_ActivateSonderkarte_AddsToActiveList()
    {
        var state = GameState.Create(rules: RuleSet.Default());
        state.Apply(new ActivateSonderkarteModification(SonderkarteType.Schweinchen));
        state.ActiveSonderkarten.Should().Contain(SonderkarteType.Schweinchen);
    }

    [Fact]
    public void Apply_ActivateSchweinchen_RebuildsTrumpEvaluator_KaroAssAboveDulle()
    {
        var state = GameState.Create(rules: RuleSet.Default());
        state.Apply(new ActivateSonderkarteModification(SonderkarteType.Schweinchen));
        state.Apply(new RebuildTrumpEvaluatorModification());

        var karoAss = new CardType(Suit.Karo, Rank.Ass);
        var dulle = new CardType(Suit.Herz, Rank.Zehn);

        state
            .TrumpEvaluator.GetTrumpRank(karoAss)
            .Should()
            .BeGreaterThan(state.TrumpEvaluator.GetTrumpRank(dulle));
    }

    [Fact]
    public void Apply_ActivateHeidfrau_SuppressesHeidmann_QueensBackAboveJacks()
    {
        var state = GameState.Create(rules: RuleSet.Default());

        state.Apply(new ActivateSonderkarteModification(SonderkarteType.Heidmann));
        state.Apply(new RebuildTrumpEvaluatorModification());

        // After Heidmann: Jacks above Queens
        var kreuzDame = new CardType(Suit.Kreuz, Rank.Dame);
        var kreuzBube = new CardType(Suit.Kreuz, Rank.Bube);
        state
            .TrumpEvaluator.GetTrumpRank(kreuzBube)
            .Should()
            .BeGreaterThan(
                state.TrumpEvaluator.GetTrumpRank(kreuzDame),
                because: "Heidmann makes Jacks outrank Queens"
            );

        state.Apply(new ActivateSonderkarteModification(SonderkarteType.Heidfrau));
        state.Apply(new RebuildTrumpEvaluatorModification());

        // After Heidfrau suppresses Heidmann: Queens back above Jacks
        state
            .TrumpEvaluator.GetTrumpRank(kreuzDame)
            .Should()
            .BeGreaterThan(
                state.TrumpEvaluator.GetTrumpRank(kreuzBube),
                because: "Heidfrau reverts Heidmann; Queens should outrank Jacks again"
            );
    }
}
