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
        result.ReAugen.Should().Be(132);
        result.KontraAugen.Should().Be(44);
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
        result.ReAugen.Should().Be(0);
    }

    // ── gameValue components ──────────────────────────────────────────────────

    [Fact]
    public void Score_GameValue_Gewonnen_MinimalGame()
    {
        // Re=132, Kontra=132; loserAugen=132 ≥ 90 → no threshold bonuses
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
        result.KontraAugen.Should().Be(132); // ≥ 90
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
        // Re=176 (4×44), Kontra=44 → loserAugen=44 < 90 → +1; < 60 → +1
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
        result.KontraAugen.Should().Be(44);
        // gameValue = 1 (Gewonnen) + 1 (Keine90) + 1 (Keine60) = 3; 44≥30 so no Keine30
        result.GameValue.Should().Be(3);
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
        result.KontraAugen.Should().Be(0);
        // gameValue = 1 + 1(Keine90) + 1(Keine60) + 1(Keine30) + 1(Schwarz) = 5
        result.GameValue.Should().Be(5);
    }

    [Fact]
    public void Score_AnnouncementAddsOnePoint()
    {
        // Re wins with 1 announcement; loserAugen=132 ≥ 90 → no threshold bonuses
        var state = SoloState(
            rules: NoFeigheit,
            announcements: [B.Ann(B.P0, AnnouncementType.Win)]
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

        result.Winner.Should().Be(Party.Re);
        result.KontraAugen.Should().Be(132); // ≥ 90
        // gameValue = 1 (Gewonnen) + 1 (announcement) = 2
        result.GameValue.Should().Be(2);
    }

    [Fact]
    public void Score_TwoAnnouncementsAddTwoPoints()
    {
        var state = SoloState(
            rules: NoFeigheit,
            announcements: [B.Ann(B.P0, AnnouncementType.Win), B.Ann(B.P1, AnnouncementType.Win)]
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

    // ── Absage: automatic loss when Absage unfulfilled ────────────────────────

    [Fact]
    public void Score_KontraWins_WhenReFailsKeine90Absage()
    {
        // Re=143, Kontra=97. Re announced keine 90, but Kontra has 97 ≥ 90 → Re loses automatically.
        var state = SoloState(
            rules: NoFeigheit,
            announcements:
            [
                B.Ann(B.P0, AnnouncementType.Win),
                B.Ann(B.P0, AnnouncementType.Keine90),
                B.Ann(B.P1, AnnouncementType.Win),
            ]
        );
        // Re: 3×44=132+11=143 (use 3 high tricks + partial), Kontra: 97
        // Simplest: 3 Re tricks (132) + Kontra trick with 44 = Re 132, Kontra 44 → not 97
        // Let's do Re=4×44=176-33=143 → too complex; use explicit Augen via trick split
        // 3 Re tricks (3×44=132), 1 Kontra trick (44), 1 more Kontra trick (44) = Re 132, Kontra 88 < 90 → test wrong
        // Instead: Re 2 tricks (88), Kontra 3 tricks (132) → Re<121 AND Re's keine90 means Kontra<90 but Kontra=132≥90 → Re loses
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P1, 8),
            B.HighValueTrick(B.P1, 12),
            B.HighValueTrick(B.P1, 16),
        };
        // Re=88, Kontra=132. Re's keine90 requires Kontra<90, but Kontra=132≥90 → Re loses.
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Winner.Should().Be(Party.Kontra);
        result.ReAugen.Should().Be(88);
        result.KontraAugen.Should().Be(132);
    }

    [Fact]
    public void Score_KontraWins_WhenReFailsKeine90_EvenIfReHas121Plus()
    {
        // Re=132, Kontra=108. Re announced keine90, but Kontra has 108 ≥ 90.
        // Even though Re would normally win (132≥121), the Absage failure makes Re lose.
        var state = SoloState(
            rules: NoFeigheit,
            announcements:
            [
                B.Ann(B.P0, AnnouncementType.Win),
                B.Ann(B.P0, AnnouncementType.Keine90),
            ]
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
        // Re=132, Kontra=132 → Kontra≥90 → Re's keine90 fails → Kontra wins
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Winner.Should().Be(Party.Kontra);
    }

    [Fact]
    public void Score_ReWins_WhenKontraFailsKeine90Absage()
    {
        // Kontra announced keine90, but Re has ≥90 → Kontra loses automatically.
        var state = SoloState(
            rules: NoFeigheit,
            announcements:
            [
                B.Ann(B.P1, AnnouncementType.Win),
                B.Ann(B.P1, AnnouncementType.Keine90),
            ]
        );
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            B.HighValueTrick(B.P1, 12),
            B.HighValueTrick(B.P1, 16),
        };
        // Re=132, Kontra=88. Kontra's keine90 requires Re<90, but Re=132≥90 → Kontra loses.
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Winner.Should().Be(Party.Re);
    }

    [Fact]
    public void Score_Keine90AbsageFulfilled_UsesNormalAugenLogic()
    {
        // Re announced keine90, Kontra has 44 < 90 → Absage fulfilled → normal Augen logic applies.
        var state = SoloState(
            rules: NoFeigheit,
            announcements:
            [
                B.Ann(B.P0, AnnouncementType.Win),
                B.Ann(B.P0, AnnouncementType.Keine90),
            ]
        );
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            B.HighValueTrick(B.P0, 12),
            B.HighValueTrick(B.P1, 16),
        };
        // Re=176, Kontra=44 < 90 → fulfilled → Re wins with normal logic
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.Winner.Should().Be(Party.Re);
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
        // Re=308 (7×44), Kontra=88 (2×44). loserAugen=88 < 90 but ≥ 60.
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

    // ── SoloFactor + TotalScore ───────────────────────────────────────────────

    [Fact]
    public void Score_SoloFactor_IsOne_WhenNoActiveReservation()
    {
        // SoloState has no ActiveReservation → SoloFactor = 1
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

        result.SoloFactor.Should().Be(1);
        result.TotalScore.Should().Be(result.GameValue); // no extra, no solo → equal
    }

    [Fact]
    public void Score_SoloFactor_IsThree_WhenSoloReservationActive()
    {
        // ActiveReservation = Bubensolo → IsSolo = true → SoloFactor = 3
        var state = GameState.Create(
            rules: NoFeigheit,
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver(),
            activeReservation: new BubensoloReservation(B.P0)
        );
        // Re=132, Kontra=132 → GameValue=1 (only Gewonnen); TotalScore = 1 × 3 = 3
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

        result.SoloFactor.Should().Be(3);
        result.GameValue.Should().Be(1);
        result.TotalScore.Should().Be(3); // 1 × 3
    }

    [Fact]
    public void Score_TotalScore_IncludesExtrapunkteOffset()
    {
        // Normal game (no solo): Re wins; Re gets +1 Extrapunkt (e.g. Doppelkopf).
        // GameValue = 1 (Gewonnen); TotalScore = 1 × 1 + 1 (Re extra) - 0 (Kontra extra) = 2.
        var state = SoloState(rules: NoFeigheit);
        var award = new ExtrapunktAward(ExtrapunktType.Doppelkopf, B.P0, 1); // Re benefits
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            new(B.HighValueTrick(B.P1, 12).Trick, B.P1, [award]), // Kontra wins but Re benefits
            B.HighValueTrick(B.P1, 16),
            B.HighValueTrick(B.P1, 20),
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.GameValue.Should().Be(1); // Extrapunkte NOT in GameValue
        result.TotalScore.Should().Be(2); // GameValue(1) × SoloFactor(1) + winnerExtra(1)
    }

    [Fact]
    public void Score_TotalScore_SoloWithExtrapunkte()
    {
        // Solo game: Re wins; Re gets +1 Extrapunkt.
        // GameValue = 1 (Gewonnen); TotalScore = 1 × 3 + 1 = 4.
        var state = GameState.Create(
            rules: NoFeigheit,
            players: B.FourPlayers(),
            partyResolver: B.SoloResolver(),
            activeReservation: new BubensoloReservation(B.P0)
        );
        var award = new ExtrapunktAward(ExtrapunktType.Doppelkopf, B.P0, 1); // Re benefits
        var tricks = new List<TrickResult>
        {
            B.HighValueTrick(B.P0, 0),
            B.HighValueTrick(B.P0, 4),
            B.HighValueTrick(B.P0, 8),
            new(B.HighValueTrick(B.P1, 12).Trick, B.P1, [award]),
            B.HighValueTrick(B.P1, 16),
            B.HighValueTrick(B.P1, 20),
        };
        var result = Sut.Score(new CompletedGame(state, tricks));

        result.SoloFactor.Should().Be(3);
        result.GameValue.Should().Be(1);
        result.TotalScore.Should().Be(6); // (GameValue 1 + winnerExtra 1) × soloFactor 3
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
