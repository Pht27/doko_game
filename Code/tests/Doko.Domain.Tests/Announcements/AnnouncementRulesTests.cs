using Doko.Domain.Tests.Helpers;

namespace Doko.Domain.Tests.Announcements;

public class AnnouncementRulesTests
{
    // ── CanAnnounce: rules disabled ───────────────────────────────────────────

    [Fact]
    public void CanAnnounce_ReturnsFalse_WhenAnnouncementsDisabled()
    {
        var state = B.BasicState(rules: RuleSet.Minimal()); // AllowAnnouncements=false
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Win, state).Should().BeFalse();
    }

    // ── CanAnnounce: timing ───────────────────────────────────────────────────

    [Fact]
    public void CanAnnounce_AllowedBeforeDeadline_ZeroCardsPlayed()
    {
        // deadline = 5 + 4*0 = 5; 0 cards played < 5 → allowed
        var state = B.BasicState();
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Win, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_AllowedAtFourCardsPlayed()
    {
        // 1 completed trick = 4 cards; deadline=5 → 4 < 5 → allowed
        var trick = CompleteTrick(B.P0);
        var state = B.BasicState(completedTricks: [trick]);
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Win, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_BlockedAtDeadline_FiveCardsPlayed()
    {
        // 1 completed trick (4) + 2 cards in current trick = 6; deadline=5 → 6 > 5 → blocked
        var completed = CompleteTrick(B.P0);
        var current = new Trick();
        current.Add(new TrickCard(B.Card(99, Suit.Kreuz, Rank.Neun), B.P0));
        current.Add(new TrickCard(B.Card(100, Suit.Kreuz, Rank.Zehn), B.P1));

        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver(),
            completedTricks: [completed],
            currentTrick: current
        );

        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Win, state).Should().BeFalse();
    }

    [Fact]
    public void CanAnnounce_DeadlineShiftsWithEachAnnouncement()
    {
        // 1 announcement made: deadline = 5 + 4*1 = 9. With 6 cards played → 6 < 9 → still allowed.
        var completed = CompleteTrick(B.P0);
        var current = new Trick();
        current.Add(new TrickCard(B.Card(99, Suit.Kreuz, Rank.Neun), B.P0));

        var win = B.Ann(B.P0, AnnouncementType.Win);
        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver(),
            completedTricks: [completed],
            currentTrick: current,
            announcements: [win]
        );

        // P1=Kontra, deadline = 9, cards = 5 → allowed to announce Win
        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Win, state).Should().BeTrue();
    }

    // ── CanAnnounce: party membership ─────────────────────────────────────────

    [Fact]
    public void CanAnnounce_RePlayer_CanAnnounceWin()
    {
        var state = B.BasicState(); // P0=Re via SoloResolver
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Win, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_KontraPlayer_CanAnnounceWin()
    {
        var state = B.BasicState();
        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Win, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_Win_BlockedWhenAlreadyAnnounced()
    {
        var state = B.BasicState(announcements: [B.Ann(B.P0, AnnouncementType.Win)]);
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Win, state).Should().BeFalse();
    }

    // ── CanAnnounce: consecutive ordering ─────────────────────────────────────

    [Fact]
    public void CanAnnounce_Keine90_BlockedWithoutWin()
    {
        var state = B.BasicState(); // no prior announcements
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Keine90, state).Should().BeFalse();
    }

    [Fact]
    public void CanAnnounce_Keine90_AllowedAfterWin()
    {
        var state = B.BasicState(announcements: [B.Ann(B.P0, AnnouncementType.Win)]);
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Keine90, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_Keine60_BlockedWithoutKeine90()
    {
        var state = B.BasicState(announcements: [B.Ann(B.P0, AnnouncementType.Win)]);
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Keine60, state).Should().BeFalse();
    }

    [Fact]
    public void CanAnnounce_Keine60_AllowedAfterKeine90()
    {
        var state = B.BasicState(
            announcements:
            [
                B.Ann(B.P0, AnnouncementType.Win),
                B.Ann(B.P0, AnnouncementType.Keine90),
            ]
        );
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Keine60, state).Should().BeTrue();
    }

    // ── GetMandatoryAnnouncement ───────────────────────────────────────────────

    [Fact]
    public void GetMandatoryAnnouncement_ReturnsNull_WhenDisabled()
    {
        var trick = HighValueTrick(B.P0);
        var state = B.BasicState(rules: RuleSet.Minimal(), completedTricks: [trick]);
        AnnouncementRules.GetMandatoryAnnouncement(B.P0, state).Should().BeNull();
    }

    [Fact]
    public void GetMandatoryAnnouncement_ReturnsNull_WhenFirstTrickBelowThreshold()
    {
        var trick = CompleteTrick(B.P0); // low-value
        var state = B.BasicState(completedTricks: [trick]);
        AnnouncementRules.GetMandatoryAnnouncement(B.P0, state).Should().BeNull();
    }

    [Fact]
    public void GetMandatoryAnnouncement_ReturnsWinMandatory_WhenFirstTrickHighAndNoAnnouncement()
    {
        var trick = HighValueTrick(B.P0); // P0 = Re (SoloResolver)
        var state = B.BasicState(completedTricks: [trick]);
        var ann = AnnouncementRules.GetMandatoryAnnouncement(B.P0, state);
        ann.Should().NotBeNull();
        ann!.Type.Should().Be(AnnouncementType.Win);
        ann.IsMandatory.Should().BeTrue();
        ann.Player.Should().Be(B.P0);
    }

    [Fact]
    public void GetMandatoryAnnouncement_ReturnsNull_WhenFirstTrickHighButAlreadyAnnounced()
    {
        var trick = HighValueTrick(B.P0);
        var state = B.BasicState(
            completedTricks: [trick],
            announcements: [B.Ann(B.P0, AnnouncementType.Win)]
        );
        AnnouncementRules.GetMandatoryAnnouncement(B.P0, state).Should().BeNull();
    }

    [Fact]
    public void GetMandatoryAnnouncement_ReturnsNull_WhenSecondTrickHighButFirstWasNot()
    {
        // Bug regression: second trick alone ≥ 35 must NOT trigger Pflichtansage
        var firstTrick = CompleteTrick(B.P0); // low
        var secondTrick = HighValueTrick(B.P0, startId: 4); // high
        var state = B.BasicState(completedTricks: [firstTrick, secondTrick]);
        AnnouncementRules.GetMandatoryAnnouncement(B.P0, state).Should().BeNull();
    }

    [Fact]
    public void GetMandatoryAnnouncement_ReturnsKeine90_WhenBothTricksHighAndWinAlreadyAnnounced()
    {
        var firstTrick = HighValueTrick(B.P0);
        var secondTrick = HighValueTrick(B.P0, startId: 4);
        var state = B.BasicState(
            completedTricks: [firstTrick, secondTrick],
            announcements: [B.Ann(B.P0, AnnouncementType.Win)]
        );
        var ann = AnnouncementRules.GetMandatoryAnnouncement(B.P0, state);
        ann.Should().NotBeNull();
        ann!.Type.Should().Be(AnnouncementType.Keine90);
        ann.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void GetMandatoryAnnouncement_ReturnsNull_AfterThirdTrick()
    {
        var t0 = HighValueTrick(B.P0);
        var t1 = HighValueTrick(B.P0, startId: 4);
        var t2 = HighValueTrick(B.P0, startId: 8);
        var state = B.BasicState(completedTricks: [t0, t1, t2]);
        AnnouncementRules.GetMandatoryAnnouncement(B.P0, state).Should().BeNull();
    }

    // ── ViolatesFeigheit: rules / solo guards ─────────────────────────────────

    [Fact]
    public void ViolatesFeigheit_ReturnsFalse_WhenFeigheitDisabled()
    {
        var state = B.BasicState(rules: RuleSet.Minimal()); // EnforceFeigheit=false
        var result = new GameResult(
            Winner: Party.Re,
            ReAugen: 180,
            KontraAugen: 60,
            ReStiche: 1,
            KontraStiche: 0,
            GameValue: 1,
            AllAwards: [],
            Feigheit: false,
            ValueComponents: [],
            SoloFactor: 1,
            TotalScore: 1,
            AnnouncementRecords: []
        );
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
        var result = new GameResult(
            Winner: Party.Re,
            ReAugen: 180,
            KontraAugen: 20,
            ReStiche: 1,
            KontraStiche: 0,
            GameValue: 1,
            AllAwards: [],
            Feigheit: false,
            ValueComponents: [],
            SoloFactor: 1,
            TotalScore: 1,
            AnnouncementRecords: []
        );
        AnnouncementRules.ViolatesFeigheit(result, state).Should().BeFalse();
    }

    // ── ViolatesFeigheit: missing-announcement counting ───────────────────────

    [Fact]
    public void ViolatesFeigheit_ReturnsFalse_WhenOnlyTwoMissing()
    {
        // Winner=Re, loserAugen=88: missing Win(+1) + Keine90(+1) = 2, not > 2 → no Feigheit.
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

        var result = new GameResult(
            Winner: Party.Re,
            ReAugen: 152,
            KontraAugen: 88,
            ReStiche: 1,
            KontraStiche: 0,
            GameValue: 1,
            AllAwards: [],
            Feigheit: false,
            ValueComponents: [],
            SoloFactor: 1,
            TotalScore: 1,
            AnnouncementRecords: []
        );
        AnnouncementRules.ViolatesFeigheit(result, state).Should().BeFalse();
    }

    [Fact]
    public void ViolatesFeigheit_ReturnsTrue_WhenThreeMissing()
    {
        // Winner=Re, loserAugen=44: missing Win + Keine90 + Keine60 = 3 → Feigheit
        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver()
        );

        var result = new GameResult(
            Winner: Party.Re,
            ReAugen: 196,
            KontraAugen: 44,
            ReStiche: 1,
            KontraStiche: 0,
            GameValue: 1,
            AllAwards: [],
            Feigheit: false,
            ValueComponents: [],
            SoloFactor: 1,
            TotalScore: 1,
            AnnouncementRecords: []
        );
        AnnouncementRules.ViolatesFeigheit(result, state).Should().BeTrue();
    }

    [Fact]
    public void ViolatesFeigheit_ReturnsFalse_WhenWinnerAnnouncedEnough()
    {
        // Winner=Re, loserAugen=44, but Re announced Win+Keine90+Keine60 → missing=0
        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver(),
            announcements:
            [
                B.Ann(B.P0, AnnouncementType.Win),
                B.Ann(B.P0, AnnouncementType.Keine90),
                B.Ann(B.P0, AnnouncementType.Keine60),
            ]
        );

        var result = new GameResult(
            Winner: Party.Re,
            ReAugen: 196,
            KontraAugen: 44,
            ReStiche: 1,
            KontraStiche: 0,
            GameValue: 1,
            AllAwards: [],
            Feigheit: false,
            ValueComponents: [],
            SoloFactor: 1,
            TotalScore: 1,
            AnnouncementRecords: []
        );
        AnnouncementRules.ViolatesFeigheit(result, state).Should().BeFalse();
    }

    // ── CanAnnounce: Absage mutex ─────────────────────────────────────────────

    [Fact]
    public void CanAnnounce_Keine90_BlockedWhenOtherPartyAlreadyHasKeine90()
    {
        // Kontra (P1) already has Keine90. Re (P0) cannot make any Absage.
        var state = B.BasicState(
            announcements:
            [
                B.Ann(B.P0, AnnouncementType.Win),
                B.Ann(B.P1, AnnouncementType.Win),
                B.Ann(B.P1, AnnouncementType.Keine90),
            ]
        );
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Keine90, state).Should().BeFalse();
    }

    [Fact]
    public void CanAnnounce_Keine90_AllowedWhenOtherPartyHasNoAbsage()
    {
        // Kontra (P1) only has Win, no Absage → Re can still announce Keine90.
        var state = B.BasicState(
            announcements: [B.Ann(B.P0, AnnouncementType.Win), B.Ann(B.P1, AnnouncementType.Win)]
        );
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Keine90, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_Keine60_BlockedWhenOtherPartyHasAbsage()
    {
        // Re (P0) has Keine90. Kontra (P1) cannot make Keine60 (or any Absage).
        var state = B.BasicState(
            announcements:
            [
                B.Ann(B.P0, AnnouncementType.Win),
                B.Ann(B.P0, AnnouncementType.Keine90),
                B.Ann(B.P1, AnnouncementType.Win),
                B.Ann(B.P1, AnnouncementType.Keine90), // this would also be blocked; assume it slipped through
            ]
        );
        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Keine60, state).Should().BeFalse();
    }

    [Fact]
    public void CanAnnounce_SameParty_CanContinueAbsageLadder()
    {
        // Re already has Keine90. Re can continue to Keine60 (other party has no Absage).
        var state = B.BasicState(
            announcements:
            [
                B.Ann(B.P0, AnnouncementType.Win),
                B.Ann(B.P0, AnnouncementType.Keine90),
            ]
        );
        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Keine60, state).Should().BeTrue();
    }

    // ── CanAnnounce: Hochzeit timing ──────────────────────────────────────────

    [Fact]
    public void CanAnnounce_Hochzeit_BlockedBeforeFindungsstich()
    {
        // No tricks completed → Findungsstich not yet found → announcements blocked for everyone
        var resolver = new HochzeitPartyResolver(B.P0, HochzeitCondition.FirstTrick);
        var state = B.BasicState(partyResolver: resolver);

        AnnouncementRules.CanAnnounce(B.P0, AnnouncementType.Win, state).Should().BeFalse();
        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Win, state).Should().BeFalse();
    }

    [Fact]
    public void CanAnnounce_Hochzeit_AllowedRightAfterFindungsstich_TrickZero()
    {
        // Findungsstich at trick 0 (K=0 → base deadline=5). 4 cards played < 5 → allowed.
        var resolver = new HochzeitPartyResolver(B.P0, HochzeitCondition.FirstTrick);
        var state = B.BasicState(
            partyResolver: resolver,
            completedTricks: [HochzeitFindungsstichP1(0)]
        );

        // P1 won the Findungsstich → P1 is Re; P2 is Kontra — both can announce Win
        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Win, state).Should().BeTrue();
        AnnouncementRules.CanAnnounce(B.P2, AnnouncementType.Win, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_Hochzeit_AllowedAfterFindungsstich_LaterTrick()
    {
        // Findungsstich at trick 2 (K=2 → base deadline=13). 3 completed tricks = 12 cards played < 13 → allowed.
        var resolver = new HochzeitPartyResolver(B.P0, HochzeitCondition.FirstTrick);
        var state = B.BasicState(
            partyResolver: resolver,
            completedTricks:
            [
                HochzeitP0WinsTrick(0),
                HochzeitP0WinsTrick(4),
                HochzeitFindungsstichP1(8),
            ]
        );

        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Win, state).Should().BeTrue();
    }

    [Fact]
    public void CanAnnounce_Hochzeit_BlockedAtDeadline_AfterFindungsstich()
    {
        // Findungsstich at trick 2 (K=2 → base deadline=13). 14 cards played (3 tricks + 2 in current) → 14 > 13 → blocked.
        var resolver = new HochzeitPartyResolver(B.P0, HochzeitCondition.FirstTrick);
        var current = new Trick();
        current.Add(new TrickCard(B.Card(99, Suit.Pik, Rank.Neun), B.P0));
        current.Add(new TrickCard(B.Card(100, Suit.Herz, Rank.Neun), B.P1));

        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: resolver,
            completedTricks:
            [
                HochzeitP0WinsTrick(0),
                HochzeitP0WinsTrick(4),
                HochzeitFindungsstichP1(8),
            ],
            currentTrick: current
        );

        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Win, state).Should().BeFalse();
    }

    [Fact]
    public void CanAnnounce_Hochzeit_DeadlineShiftsWithAnnouncement()
    {
        // Findungsstich at trick 2 (K=2 → base deadline=13). 1 prior announcement → effective deadline=17.
        // 13 cards played < 17 → still allowed.
        var resolver = new HochzeitPartyResolver(B.P0, HochzeitCondition.FirstTrick);
        var current = new Trick();
        current.Add(new TrickCard(B.Card(99, Suit.Pik, Rank.Neun), B.P0));

        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: resolver,
            completedTricks:
            [
                HochzeitP0WinsTrick(0),
                HochzeitP0WinsTrick(4),
                HochzeitFindungsstichP1(8),
            ],
            currentTrick: current,
            announcements: [B.Ann(B.P1, AnnouncementType.Win)]
        );

        // P1 already announced Win; now Keine90 is next. Deadline=17, 13 cards → allowed.
        AnnouncementRules.CanAnnounce(B.P1, AnnouncementType.Keine90, state).Should().BeTrue();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>A trick where P0 (Hochzeit player) wins by leading ♠A.</summary>
    private static Trick HochzeitP0WinsTrick(byte startId) =>
        B.Trick(
            (startId, Suit.Pik, Rank.Ass, B.P0),
            ((byte)(startId + 1), Suit.Herz, Rank.Neun, B.P1),
            ((byte)(startId + 2), Suit.Herz, Rank.Koenig, B.P2),
            ((byte)(startId + 3), Suit.Herz, Rank.Ass, B.P3)
        );

    /// <summary>The Findungsstich: P0 leads low ♠9, P1 wins with ♠A.</summary>
    private static Trick HochzeitFindungsstichP1(byte startId) =>
        B.Trick(
            (startId, Suit.Pik, Rank.Neun, B.P0),
            ((byte)(startId + 1), Suit.Pik, Rank.Ass, B.P1),
            ((byte)(startId + 2), Suit.Herz, Rank.Neun, B.P2),
            ((byte)(startId + 3), Suit.Herz, Rank.Koenig, B.P3)
        );

    /// <summary>
    /// A trick where <paramref name="winner"/> wins ~43 Augen: ♣A, ♣10, ♠A, ♥A.
    /// Card IDs start at <paramref name="startId"/> to avoid conflicts in multi-trick states.
    /// </summary>
    private static Trick HighValueTrick(PlayerSeat winner, byte startId = 10)
    {
        var trick = new Trick();
        trick.Add(new TrickCard(B.Card(startId, Suit.Kreuz, Rank.Ass), winner));
        trick.Add(new TrickCard(B.Card((byte)(startId + 1), Suit.Kreuz, Rank.Zehn), B.P1));
        trick.Add(new TrickCard(B.Card((byte)(startId + 2), Suit.Pik, Rank.Ass), B.P2));
        trick.Add(new TrickCard(B.Card((byte)(startId + 3), Suit.Herz, Rank.Ass), B.P3));
        return trick;
    }

    /// <summary>A low-value 4-card trick.</summary>
    private static Trick CompleteTrick(PlayerSeat winner)
    {
        var trick = new Trick();
        trick.Add(new TrickCard(B.Card(20, Suit.Kreuz, Rank.Ass), winner));
        trick.Add(new TrickCard(B.Card(21, Suit.Kreuz, Rank.Neun), B.P1));
        trick.Add(new TrickCard(B.Card(22, Suit.Pik, Rank.Neun), B.P2));
        trick.Add(new TrickCard(B.Card(23, Suit.Herz, Rank.Neun), B.P3));
        return trick;
    }
}
