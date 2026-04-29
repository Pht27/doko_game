using Doko.Domain.GameFlow.Modifications;
using Doko.Domain.Tests.Helpers;

namespace Doko.Domain.Tests.GameFlow;

public class GameStateTests
{
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

    // ── Apply: SetGenscherPartnerModification in silent solos ─────────────────

    [Fact]
    public void Apply_Genscher_InKontraSolo_DoesNotReplacePartyResolver()
    {
        var state = B.BasicState();
        state.Apply(
            new SetSilentGameModeModification(
                new SilentGameMode(SilentGameModeType.KontraSolo, B.P0)
            )
        );
        var resolverBefore = state.PartyResolver;

        // P0 is Kontra, P1 is Re — teams would change in a normal game
        state.Apply(new SetGenscherPartnerModification(B.P0, B.P1));

        state
            .PartyResolver.Should()
            .BeSameAs(resolverBefore, because: "KontraSolo party structure takes precedence");
        state.PartyResolver.Should().BeOfType<KontraSoloPartyResolver>();
    }

    [Fact]
    public void Apply_Genscher_InKontraSolo_DoesNotSetGenscherTeamsChanged()
    {
        var state = B.BasicState();
        state.Apply(
            new SetSilentGameModeModification(
                new SilentGameMode(SilentGameModeType.KontraSolo, B.P0)
            )
        );

        state.Apply(new SetGenscherPartnerModification(B.P0, B.P1));

        (state.Genscher?.TeamsChanged ?? false)
            .Should()
            .BeFalse(because: "all Genscher side effects are suppressed in silent solos");
    }

    [Fact]
    public void Apply_Genscher_InStilleHochzeit_DoesNotReplacePartyResolver()
    {
        var state = B.BasicState();
        state.Apply(
            new SetSilentGameModeModification(
                new SilentGameMode(SilentGameModeType.StilleHochzeit, B.P0)
            )
        );
        var resolverBefore = state.PartyResolver;

        // P0 is Re (solo), P1 is Kontra — teams would change in a normal game
        state.Apply(new SetGenscherPartnerModification(B.P1, B.P0));

        state
            .PartyResolver.Should()
            .BeSameAs(resolverBefore, because: "StilleHochzeit party structure takes precedence");
        state.PartyResolver.Should().BeOfType<StilleHochzeitPartyResolver>();
    }
}
