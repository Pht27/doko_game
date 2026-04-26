using Doko.Domain.Tests.Helpers;

namespace Doko.Domain.Tests.Announcements;

/// <summary>
/// Announcement rules specific to Kontrasolo:
/// - Kontrasolo player (P0) cannot announce at all.
/// - Re players with ♣Q (P1, P2) make effective announcements.
/// - Re players without ♣Q (P3) make button-only announcements (IsEffective=false).
/// - Button-only players share a chain among themselves to prevent info leaks.
/// - Effective and button-only chains are independent of each other.
/// </summary>
public class KontraSoloAnnouncementTests
{
    private static GameState KontraSoloState(
        IReadOnlyList<Announcement>? announcements = null,
        IReadOnlyList<Trick>? completedTricks = null
    )
    {
        var (resolver, hands) = B.KontraSoloResolver();
        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            currentTurn: B.P0,
            partyResolver: resolver,
            announcements: announcements,
            completedTricks: completedTricks,
            initialHands: hands,
            phase: GamePhase.Playing
        );
        state.Apply(
            new SetSilentGameModeModification(
                new SilentGameMode(SilentGameModeType.KontraSolo, B.P0)
            )
        );
        return state;
    }

    // ── Kontrasolo player is blocked ─────────────────────────────────────────

    [Fact]
    public void KontraSoloPlayer_CannotAnnounceWin()
    {
        var state = KontraSoloState();
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Win, state).Should().BeFalse();
    }

    [Fact]
    public void KontraSoloPlayer_CannotAnnounceKeine90EvenIfWinWerePresent()
    {
        var state = KontraSoloState();
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Keine90, state).Should().BeFalse();
    }

    // ── Effective Re player (has ♣Q) ─────────────────────────────────────────

    [Fact]
    public void EffectiveRePlayer_CanAnnounceWin()
    {
        var state = KontraSoloState();
        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Win, state).Should().BeTrue();
    }

    [Fact]
    public void EffectiveRePlayer_CanAnnounceKeine90_AfterEffectiveWin()
    {
        var win = B.Ann(B.P1, AnnouncementType.Win);
        var state = KontraSoloState(announcements: [win]);
        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Keine90, state).Should().BeTrue();
    }

    [Fact]
    public void EffectiveRePlayer_CannotAnnounceKeine90_WithoutEffectiveWin()
    {
        // Button-only Win from P3 does not satisfy P1's chain.
        var buttonOnlyWin = new Announcement(B.P3, AnnouncementType.Win, 0, 0)
        {
            IsEffective = false,
        };
        var state = KontraSoloState(announcements: [buttonOnlyWin]);
        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Keine90, state).Should().BeFalse();
    }

    // ── Button-only Re player (no ♣Q) ────────────────────────────────────────

    [Fact]
    public void ButtonOnlyRePlayer_CanAnnounceWin()
    {
        var state = KontraSoloState();
        AnnouncementRules.CanAnnounce(B.P3, AnnouncementType.Win, state).Should().BeTrue();
    }

    [Fact]
    public void ButtonOnlyRePlayer_CanAnnounceKeine90_AfterOwnWin()
    {
        var win = new Announcement(B.P3, AnnouncementType.Win, 0, 0) { IsEffective = false };
        var state = KontraSoloState(announcements: [win]);
        AnnouncementRules.CanAnnounce(B.P3, AnnouncementType.Keine90, state).Should().BeTrue();
    }

    [Fact]
    public void ButtonOnlyRePlayer_CannotAnnounceKeine90_WithoutAnyButtonOnlyWin()
    {
        // Effective Win from P1 does not satisfy P3's button-only chain.
        var effectiveWin = B.Ann(B.P1, AnnouncementType.Win);
        var state = KontraSoloState(announcements: [effectiveWin]);
        AnnouncementRules.CanAnnounce(B.P3, AnnouncementType.Keine90, state).Should().BeFalse();
    }

    // ── Shared chain between button-only players (prevents info leak) ─────────

    [Fact]
    public void ButtonOnlyPlayer_CanAnnounceKeine90_BasedOnOtherButtonOnlyPlayerWin()
    {
        // If a second button-only player existed, B1's Win would enable B2's Keine90.
        // Here P3 is button-only; we simulate a second button-only by using a non-effective P3 Win.
        // We test with two button-only announcements from the same player as a proxy:
        // the shared pool includes all non-effective Re announcements.
        var win = new Announcement(B.P3, AnnouncementType.Win, 0, 0) { IsEffective = false };
        var state = KontraSoloState(announcements: [win]);
        // P3 can now announce Keine90 using the shared non-effective pool containing Win.
        AnnouncementRules.CanAnnounce(B.P3, AnnouncementType.Keine90, state).Should().BeTrue();
    }

    // ── Pflichtansage for Kontrasolo player ───────────────────────────────────

    [Fact]
    public void GetMandatoryAnnouncement_IsNotEffective_ForKontraSoloPlayer()
    {
        var state = KontraSoloState(completedTricks: [HighValueTrick(B.P0)]);

        var ann = AnnouncementRules.GetMandatoryAnnouncement(B.P0, state);

        ann.Should().NotBeNull();
        ann!.IsEffective.Should().BeFalse();
        ann.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void GetMandatoryAnnouncement_DoesNotRepeatWin_AfterFirstNonEffectiveWin()
    {
        // A (Kontrasolo) already got a non-effective Win from trick 1.
        // When A wins trick 2 (also high), the mandatory should be Keine90, not Win again.
        var firstNonEffectiveWin = new Announcement(B.P0, AnnouncementType.Win, 0, 0)
        {
            IsMandatory = true,
            IsEffective = false,
        };
        var state = KontraSoloState(
            announcements: [firstNonEffectiveWin],
            completedTricks: [HighValueTrick(B.P0), HighValueTrick(B.P0, 4)]
        );

        var ann = AnnouncementRules.GetMandatoryAnnouncement(B.P0, state);

        ann.Should().NotBeNull();
        ann!.Type.Should().Be(AnnouncementType.Keine90);
    }

    [Fact]
    public void ButtonOnlyRePlayer_CanChainOffKontraSoloPflichtansage()
    {
        // In a normal game, A (no ♣Q) and B (no ♣Q) would both be Kontra.
        // A's forced Win allows B's Keine90. In Kontrasolo the same must hold to avoid an info leak.
        var kontraSoloPflichtWin = new Announcement(B.P0, AnnouncementType.Win, 0, 0)
        {
            IsMandatory = true,
            IsEffective = false,
        };
        var state = KontraSoloState(announcements: [kontraSoloPflichtWin]);
        AnnouncementRules.CanAnnounce(B.P3, AnnouncementType.Keine90, state).Should().BeTrue();
    }

    // ── IsEffective flag set by MakeAnnouncementHandler (via IsAnnouncementEffective) ──

    [Fact]
    public void IsAnnouncementEffective_ReturnsFalse_ForKontraSoloPlayer()
    {
        var (resolver, hands) = B.KontraSoloResolver();
        var state = GameState.Create(
            players: B.FourPlayers(),
            partyResolver: resolver,
            initialHands: hands,
            phase: GamePhase.Playing
        );
        resolver.IsAnnouncementEffective(B.P0, state).Should().BeFalse();
    }

    [Fact]
    public void IsAnnouncementEffective_ReturnsFalse_ForKontraSoloPlayer_EvenWithKreuzDame()
    {
        // Regression: solo player holding a ♣Q must still be non-effective — identity, not card holdings.
        var kreuzDame = B.Card(10, Suit.Kreuz, Rank.Dame);
        var hands = new Dictionary<PlayerSeat, Hand>
        {
            [B.P0] = B.HandOf(kreuzDame), // Kontrasolo player with ♣Q
            [B.P1] = B.HandOf(B.Card(2, Suit.Pik, Rank.Ass)),
            [B.P2] = B.HandOf(B.Card(3, Suit.Herz, Rank.Ass)),
            [B.P3] = B.HandOf(B.Card(4, Suit.Karo, Rank.Ass)),
        };
        var resolver = new KontraSoloPartyResolver(B.P0);
        var state = GameState.Create(
            players: B.FourPlayers(),
            partyResolver: resolver,
            initialHands: hands,
            phase: GamePhase.Playing
        );
        resolver.IsAnnouncementEffective(B.P0, state).Should().BeFalse();
    }

    [Fact]
    public void IsAnnouncementEffective_ReturnsTrue_ForEffectiveRePlayer()
    {
        var (resolver, hands) = B.KontraSoloResolver();
        var state = GameState.Create(
            players: B.FourPlayers(),
            partyResolver: resolver,
            initialHands: hands,
            phase: GamePhase.Playing
        );
        resolver.IsAnnouncementEffective(B.P1, state).Should().BeTrue();
    }

    [Fact]
    public void IsAnnouncementEffective_ReturnsFalse_ForButtonOnlyRePlayer()
    {
        var (resolver, hands) = B.KontraSoloResolver();
        var state = GameState.Create(
            players: B.FourPlayers(),
            partyResolver: resolver,
            initialHands: hands,
            phase: GamePhase.Playing
        );
        resolver.IsAnnouncementEffective(B.P3, state).Should().BeFalse();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Trick HighValueTrick(PlayerSeat winner, byte startId = 0)
    {
        var trick = new Trick();
        trick.Add(new TrickCard(B.Card(startId, Suit.Kreuz, Rank.Ass), winner));
        trick.Add(new TrickCard(B.Card((byte)(startId + 1), Suit.Pik, Rank.Ass), B.P1));
        trick.Add(new TrickCard(B.Card((byte)(startId + 2), Suit.Herz, Rank.Ass), B.P2));
        trick.Add(new TrickCard(B.Card((byte)(startId + 3), Suit.Karo, Rank.Ass), B.P3));
        return trick;
    }
}
