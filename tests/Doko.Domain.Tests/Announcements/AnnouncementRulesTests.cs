using Doko.Domain.Tests.Helpers;

namespace Doko.Domain.Tests.Announcements;

public class AnnouncementRulesTests
{
    // ── CanAnnounce: rules disabled ───────────────────────────────────────────

    [Fact]
    public void CanAnnounce_ReturnsFalse_WhenAnnouncementsDisabled()
    {
        var state = B.BasicState(rules: RuleSet.Minimal()); // AllowAnnouncements=false
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Re, state).Should().BeFalse();
    }

    // ── CanAnnounce: timing ───────────────────────────────────────────────────

    [Fact]
    public void CanAnnounce_AllowedBeforeDeadline_ZeroCardsPlayed()
    {
        // deadline = 5 + 4*0 = 5; 0 cards played < 5 → allowed
        var state = B.BasicState();
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Re, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_AllowedAtFourCardsPlayed()
    {
        // 1 completed trick = 4 cards; deadline=5 → 4 < 5 → allowed
        var trick = CompleteTrick(B.P0);
        var state = B.BasicState(completedTricks: [trick]);
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Re, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_BlockedAtDeadline_FiveCardsPlayed()
    {
        // 1 completed trick (4) + 1 card in current trick = 5; deadline=5 → 5 >= 5 → blocked
        var completed = CompleteTrick(B.P0);
        var current = new Trick();
        current.Add(new TrickCard(B.Card(99, Suit.Kreuz, Rank.Neun), B.P0));

        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver(),
            completedTricks: [completed],
            currentTrick: current
        );

        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Re, state).Should().BeFalse();
    }

    [Fact]
    public void CanAnnounce_DeadlineShiftsWithEachAnnouncement()
    {
        // 1 announcement made: deadline = 5 + 4*1 = 9. With 5 cards played → 5 < 9 → still allowed.
        var completed = CompleteTrick(B.P0);
        var current = new Trick();
        current.Add(new TrickCard(B.Card(99, Suit.Kreuz, Rank.Neun), B.P0));

        var re = B.Ann(B.P0, AnnouncementType.Re);
        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver(),
            completedTricks: [completed],
            currentTrick: current,
            announcements: [re]
        );

        // P1=Kontra, deadline = 9, cards = 5 → allowed to announce Kontra
        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Kontra, state).Should().BeTrue();
    }

    // ── CanAnnounce: party membership ─────────────────────────────────────────

    [Fact]
    public void CanAnnounce_RePlayer_CanAnnounceRe()
    {
        var state = B.BasicState(); // P0=Re via SoloResolver
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Re, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_RePlayer_CannotAnnounceKontra()
    {
        var state = B.BasicState();
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Kontra, state).Should().BeFalse();
    }

    [Fact]
    public void CanAnnounce_KontraPlayer_CanAnnounceKontra()
    {
        var state = B.BasicState();
        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Kontra, state).Should().BeTrue();
    }

    // ── CanAnnounce: consecutive ordering ─────────────────────────────────────

    [Fact]
    public void CanAnnounce_Keine90_BlockedWithoutRe()
    {
        var state = B.BasicState(); // no prior announcements
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Keine90, state).Should().BeFalse();
    }

    [Fact]
    public void CanAnnounce_Keine90_AllowedAfterRe()
    {
        var state = B.BasicState(announcements: [B.Ann(B.P0, AnnouncementType.Re)]);
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Keine90, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_Keine60_BlockedWithoutKeine90()
    {
        var state = B.BasicState(announcements: [B.Ann(B.P0, AnnouncementType.Re)]);
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Keine60, state).Should().BeFalse();
    }

    [Fact]
    public void CanAnnounce_Keine60_AllowedAfterKeine90()
    {
        var state = B.BasicState(
            announcements: [B.Ann(B.P0, AnnouncementType.Re), B.Ann(B.P0, AnnouncementType.Keine90)]
        );
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Keine60, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_Re_BlockedWhenAlreadyAnnounced()
    {
        var state = B.BasicState(announcements: [B.Ann(B.P0, AnnouncementType.Re)]);
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Re, state).Should().BeFalse();
    }

    // ── IsMandatory: rules disabled ───────────────────────────────────────────

    [Fact]
    public void IsMandatory_ReturnsFalse_WhenPflichtansageDisabled()
    {
        var trick = HighValueTrick(B.P0);
        var state = B.BasicState(
            rules: RuleSet.Minimal(), // EnforcePflichtansage=false
            completedTricks: [trick]
        );
        AnnouncementRules.IsMandatory(B.P0, state).Should().BeFalse();
    }

    // ── IsMandatory: no tricks ────────────────────────────────────────────────

    [Fact]
    public void IsMandatory_ReturnsFalse_WhenNoTricksCompleted()
    {
        var state = B.BasicState(); // completedTricks = []
        AnnouncementRules.IsMandatory(B.P0, state).Should().BeFalse();
    }

    // ── IsMandatory: first trick thresholds ───────────────────────────────────

    [Fact]
    public void IsMandatory_ReturnsFalse_WhenFirstTrickPointsBelowThreshold()
    {
        // Kreuz Ass (11) leads, 3 Nines follow → 11 Augen < 35
        var trick = B.Trick(
            (0, Suit.Kreuz, Rank.Ass, B.P0),
            (1, Suit.Kreuz, Rank.Neun, B.P1),
            (2, Suit.Pik, Rank.Neun, B.P2),
            (3, Suit.Herz, Rank.Neun, B.P3)
        );
        var state = B.BasicState(completedTricks: [trick]);
        AnnouncementRules.IsMandatory(B.P0, state).Should().BeFalse();
    }

    [Fact]
    public void IsMandatory_ReturnsTrue_WhenFirstTrickWinnerHas35PlusAndNoAnnouncement()
    {
        var trick = HighValueTrick(B.P0); // P0 wins 43-Augen trick
        var state = B.BasicState(completedTricks: [trick]);
        AnnouncementRules.IsMandatory(B.P0, state).Should().BeTrue();
    }

    [Fact]
    public void IsMandatory_ReturnsFalse_WhenFirstTrickWinnerAlreadyAnnounced()
    {
        var trick = HighValueTrick(B.P0);
        var state = B.BasicState(
            completedTricks: [trick],
            announcements: [B.Ann(B.P0, AnnouncementType.Re)]
        );
        AnnouncementRules.IsMandatory(B.P0, state).Should().BeFalse();
    }

    [Fact]
    public void IsMandatory_ReturnsFalse_WhenFirstTrickWinnerIsOtherPlayer()
    {
        var trick = HighValueTrick(B.P1); // P1 wins, not P0
        var state = B.BasicState(completedTricks: [trick]);
        AnnouncementRules.IsMandatory(B.P0, state).Should().BeFalse();
    }

    // ── ViolatesFeigheit: rules / solo guards ─────────────────────────────────

    [Fact]
    public void ViolatesFeigheit_ReturnsFalse_WhenFeigheitDisabled()
    {
        var state = B.BasicState(rules: RuleSet.Minimal()); // EnforceFeigheit=false
        var result = new GameResult(Party.Re, 180, 60, 1, [], Feigheit: false);
        AnnouncementRules.ViolatesFeigheit(result, state).Should().BeFalse();
    }

    [Fact]
    public void ViolatesFeigheit_ReturnsFalse_InSoloGame()
    {
        // ActiveReservation != null → skip Feigheit check
        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver(),
            activeReservation: new DamensoloReservation(B.P0)
        );
        var result = new GameResult(Party.Re, 180, 20, 1, [], Feigheit: false);
        AnnouncementRules.ViolatesFeigheit(result, state).Should().BeFalse();
    }

    // ── ViolatesFeigheit: missing-announcement counting ───────────────────────

    [Fact]
    public void ViolatesFeigheit_ReturnsFalse_WhenOnlyTwoMissing()
    {
        // Winner=Re, loserPoints=88: missing Re(+1) + Keine90(+1) = 2, not > 2 → no Feigheit.
        // State includes a Kontra trick so loserWonNoTricks=false (avoids Schwarz missing point).
        var kontraTrick = B.Trick(
            (30, Suit.Kreuz, Rank.Ass, B.P1),
            (31, Suit.Kreuz, Rank.Neun, B.P0),
            (32, Suit.Pik, Rank.Neun, B.P2),
            (33, Suit.Herz, Rank.Neun, B.P3)
        );

        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver(),
            completedTricks: [kontraTrick]
        );

        var result = new GameResult(Party.Re, 152, 88, 1, [], Feigheit: false);
        AnnouncementRules.ViolatesFeigheit(result, state).Should().BeFalse();
    }

    [Fact]
    public void ViolatesFeigheit_ReturnsTrue_WhenThreeMissing()
    {
        // Winner=Re, loserPoints=44: missing Re + Keine90 + Keine60 = 3 → Feigheit
        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver()
        );

        var result = new GameResult(Party.Re, 196, 44, 1, [], Feigheit: false);
        AnnouncementRules.ViolatesFeigheit(result, state).Should().BeTrue();
    }

    [Fact]
    public void ViolatesFeigheit_ReturnsFalse_WhenWinnerAnnouncedEnough()
    {
        // Winner=Re, loserPoints=44, but Re announced Re+Keine90+Keine60 → missing=0
        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver(),
            announcements:
            [
                B.Ann(B.P0, AnnouncementType.Re),
                B.Ann(B.P0, AnnouncementType.Keine90),
                B.Ann(B.P0, AnnouncementType.Keine60),
            ]
        );

        var result = new GameResult(Party.Re, 196, 44, 1, [], Feigheit: false);
        AnnouncementRules.ViolatesFeigheit(result, state).Should().BeFalse();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// A trick where P0 wins ~43 Augen: leads ♣A (plain), ♣10, ♠A, ♥A follow.
    /// P0 wins (highest ♣ and no trump played).
    /// </summary>
    private static Trick HighValueTrick(PlayerId winner)
    {
        // Use B.P0 as first card to ensure P0 wins when winner=P0
        var trick = new Trick();
        trick.Add(new TrickCard(B.Card(10, Suit.Kreuz, Rank.Ass), winner));
        trick.Add(new TrickCard(B.Card(11, Suit.Kreuz, Rank.Zehn), B.P1));
        trick.Add(new TrickCard(B.Card(12, Suit.Pik, Rank.Ass), B.P2));
        trick.Add(new TrickCard(B.Card(13, Suit.Herz, Rank.Ass), B.P3));
        return trick;
    }

    /// <summary>A low-value 4-card trick led by P0.</summary>
    private static Trick CompleteTrick(PlayerId winner)
    {
        var trick = new Trick();
        trick.Add(new TrickCard(B.Card(20, Suit.Kreuz, Rank.Ass), winner));
        trick.Add(new TrickCard(B.Card(21, Suit.Kreuz, Rank.Neun), B.P1));
        trick.Add(new TrickCard(B.Card(22, Suit.Pik, Rank.Neun), B.P2));
        trick.Add(new TrickCard(B.Card(23, Suit.Herz, Rank.Neun), B.P3));
        return trick;
    }
}
