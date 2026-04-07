using Doko.Domain.Tests.Helpers;

namespace Doko.Domain.Tests.Scoring;

public class GameScorerTests
{
    private static readonly GameScorer Sut = new();

    // Feigheit-free ruleset to isolate non-Feigheit tests from interference.
    private static readonly RuleSet NoFeigheit = RuleSet.Default() with { EnforceFeigheit = false };

    // ── Re wins ───────────────────────────────────────────────────────────────

    [Fact]
    public void Score_ReWins_WhenReReaches121()
    {
        // Re (P0) gets 3×44=132 Augen; Kontra (P1) gets 1×44=44 Augen
        var state = SoloState(rules: NoFeigheit);
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            B.HighValueTrick(B.P1, 12),
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Winner.Should().Be(Party.Re);
        result.RePoints.Should().Be(132);
        result.KontraPoints.Should().Be(44);
    }

    [Fact]
    public void Score_KontraWins_WhenReBelowThreshold()
    {
        // Kontra (P1) takes all Augen; Re (P0) has nothing
        var state = SoloState(rules: NoFeigheit);
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P1, 0),
            B.HighValueTrick(B.P1, 4),
            B.HighValueTrick(B.P1, 8),
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Winner.Should().Be(Party.Kontra);
        result.RePoints.Should().Be(0);
    }

    // ── gameValue components ──────────────────────────────────────────────────

    [Fact]
    public void Score_GameValue_Gewonnen_MinimalGame()
    {
        // Re=132, Kontra=132; loserPoints=132 ≥ 90 → no threshold bonuses
        var state = SoloState(rules: NoFeigheit);
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            B.HighValueTrick(B.P1, 12),
            B.HighValueTrick(B.P1, 16),
            B.HighValueTrick(B.P1, 20),
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Winner.Should().Be(Party.Re);
        result.KontraPoints.Should().Be(132); // ≥ 90
        result.GameValue.Should().Be(1); // only Gewonnen
    }

    [Fact]
    public void Score_GegenDieAlten_AddedWhenKontraWins()
    {
        // Kontra takes majority; Re=44 < 121 → Kontra wins
        var state = SoloState(rules: NoFeigheit);
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P1, 0),
            B.HighValueTrick(B.P1, 4),
            B.HighValueTrick(B.P1, 8),
            B.HighValueTrick(B.P0, 12),
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Winner.Should().Be(Party.Kontra);
        // gameValue = 1 (Gewonnen) + 1 (Gegen die Alten) + threshold bonuses for Re<90
        result.GameValue.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void Score_Keine90_Bonus_WhenLoserBelow90()
    {
        // Re=176 (4×44), Kontra=44 → loserPoints=44 < 90 → +1; < 60 → +1
        var state = SoloState(rules: NoFeigheit);
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            B.HighValueTrick(B.P0, 12),
            B.HighValueTrick(B.P1, 16), // Kontra: 44
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Winner.Should().Be(Party.Re);
        result.KontraPoints.Should().Be(44);
        // gameValue = 1 (Gewonnen) + 1 (Keine90) + 1 (Keine60) = 3; 44≥30 so no Keine30
        result.GameValue.Should().Be(3);
    }

    [Fact]
    public void Score_Keine30_And_NoSchwarz_WhenLoserBelowThirtyButWonTrick()
    {
        // Kontra wins a zero-Augen trick, so loserWonNoTricks=false (no Schwarz).
        // Re wins 4×44=176 Augen; Kontra wins 0 Augen.
        var state = SoloState(rules: NoFeigheit);
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            B.HighValueTrick(B.P0, 12),
            B.HighValueTrick(B.P0, 16),
            B.ZeroValueTrick(B.P1, 0), // Kontra wins 0 Augen
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Winner.Should().Be(Party.Re);
        result.KontraPoints.Should().Be(0); // 0 Augen
        // gameValue = 1 + 1(Keine90) + 1(Keine60) + 1(Keine30) = 4; no Schwarz (loser won a trick)
        result.GameValue.Should().Be(4);
    }

    [Fact]
    public void Score_Schwarz_WhenLoserWinsNoTricks()
    {
        // All tricks go to Re (P0) — Kontra wins nothing
        var state = SoloState(rules: NoFeigheit);
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            B.HighValueTrick(B.P0, 12),
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Winner.Should().Be(Party.Re);
        result.KontraPoints.Should().Be(0);
        // gameValue = 1 + 1(Keine90) + 1(Keine60) + 1(Keine30) + 1(Schwarz) = 5
        result.GameValue.Should().Be(5);
    }

    [Fact]
    public void Score_AnnouncementAddsOnePoint()
    {
        // Re wins with 1 announcement; loserPoints=132 ≥ 90 → no threshold bonuses
        var state = SoloState(rules: NoFeigheit, announcements: [B.Ann(B.P0, AnnouncementType.Re)]);
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            B.HighValueTrick(B.P1, 12),
            B.HighValueTrick(B.P1, 16),
            B.HighValueTrick(B.P1, 20),
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Winner.Should().Be(Party.Re);
        result.KontraPoints.Should().Be(132); // ≥ 90
        // gameValue = 1 (Gewonnen) + 1 (announcement) = 2
        result.GameValue.Should().Be(2);
    }

    [Fact]
    public void Score_TwoAnnouncementsAddTwoPoints()
    {
        var state = SoloState(
            rules: NoFeigheit,
            announcements: [B.Ann(B.P0, AnnouncementType.Re), B.Ann(B.P1, AnnouncementType.Kontra)]
        );
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            B.HighValueTrick(B.P1, 12),
            B.HighValueTrick(B.P1, 16),
            B.HighValueTrick(B.P1, 20),
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        // gameValue = 1 (Gewonnen) + 2 (announcements) = 3
        result.GameValue.Should().Be(3);
    }

    // ── Feigheit ──────────────────────────────────────────────────────────────

    [Fact]
    public void Score_Feigheit_FlipsWinner_WhenMoreThanTwoMissing()
    {
        // Re wins 176 vs 44. No announcements → missing Re+Keine90+Keine60 = 3 → Feigheit.
        // We also give Kontra a zero trick so loserWonNoTricks=false (avoid extra Schwarz missing).
        var state = SoloState();
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            B.HighValueTrick(B.P0, 12),
            B.ZeroValueTrick(B.P1, 16), // Kontra wins 0 Augen → loserWonNoTricks=false
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Feigheit.Should().BeTrue();
        result.Winner.Should().Be(Party.Kontra); // flipped
    }

    [Fact]
    public void Score_NoFeigheit_WhenOnlyTwoMissing()
    {
        // Re=308 (7×44), Kontra=88 (2×44). loserPoints=88 < 90 but ≥ 60.
        // Missing: Re(+1) + Keine90(+1) = 2 → not > 2 → no Feigheit.
        // We include a Kontra trick in state.CompletedTricks so loserWonNoTricks=false.
        var kontraTrickInState = B.Trick(
            (50, Suit.Kreuz, Rank.Ass, B.P1),
            (51, Suit.Kreuz, Rank.Neun, B.P0),
            (52, Suit.Pik, Rank.Neun, B.P2),
            (53, Suit.Herz, Rank.Neun, B.P3)
        );

        var state = GameState.Create(
            rules: RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver(),
            completedTricks: [kontraTrickInState]
        );

        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            B.HighValueTrick(B.P0, 12),
            B.HighValueTrick(B.P0, 16),
            B.HighValueTrick(B.P0, 20),
            B.HighValueTrick(B.P0, 24),
            B.HighValueTrick(B.P1, 28), // Kontra: 44
            B.HighValueTrick(B.P1, 32), // Kontra: 44 → total 88
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Feigheit.Should().BeFalse();
        result.Winner.Should().Be(Party.Re);
    }

    [Fact]
    public void Score_Feigheit_NotApplied_WhenRulesDisableIt()
    {
        var state = SoloState(rules: NoFeigheit);
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            B.HighValueTrick(B.P0, 12),
            B.ZeroValueTrick(B.P1, 16),
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Feigheit.Should().BeFalse();
        result.Winner.Should().Be(Party.Re);
    }

    // ── CardPointTransfers (Schatz) ───────────────────────────────────────────

    [Fact]
    public void Score_SchatzTransfer_ShiftsAugenBetweenCards()
    {
        // Transfer ♦A (11 pts) to ♣A: ♦A worth 0, ♣A worth 11+11=22.
        // Re wins the ♣A trick; Kontra wins the ♦A trick.
        // Without transfer: Re=11, Kontra=11. With transfer: Re=22, Kontra=0.
        var state = SoloState(rules: NoFeigheit);
        state.Apply(
            new TransferCardPointsModification(
                new CardType(Suit.Karo, Rank.Ass), // from ♦A
                new CardType(Suit.Kreuz, Rank.Ass)
            )
        ); // to ♣A

        var reTrick = new Trick();
        reTrick.Add(new TrickCard(B.Card(0, Suit.Kreuz, Rank.Ass), B.P0));
        reTrick.Add(new TrickCard(B.Card(1, Suit.Kreuz, Rank.Neun), B.P1));
        reTrick.Add(new TrickCard(B.Card(2, Suit.Kreuz, Rank.Neun), B.P2));
        reTrick.Add(new TrickCard(B.Card(3, Suit.Kreuz, Rank.Neun), B.P3));

        var kontraTrick = new Trick();
        kontraTrick.Add(new TrickCard(B.Card(4, Suit.Karo, Rank.Ass), B.P1));
        kontraTrick.Add(new TrickCard(B.Card(5, Suit.Kreuz, Rank.Neun), B.P0));
        kontraTrick.Add(new TrickCard(B.Card(6, Suit.Kreuz, Rank.Neun), B.P2));
        kontraTrick.Add(new TrickCard(B.Card(7, Suit.Kreuz, Rank.Neun), B.P3));

        var tricks = new List<TrickResult>
        {
            new(reTrick, B.P0, []), // Re wins ♣A trick
            new(kontraTrick, B.P1, []), // Kontra wins ♦A trick
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.RePoints.Should().Be(22); // ♣A = 11 + 11 (absorbed ♦A points)
        result.KontraPoints.Should().Be(0); // ♦A = 0 (transferred away)
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static GameState SoloState(
        RuleSet? rules = null,
        IReadOnlyList<Announcement>? announcements = null
    ) =>
        GameState.Create(
            rules: rules ?? RuleSet.Default(),
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver(), // P0=Re, P1/P2/P3=Kontra
            announcements: announcements
        );
}
