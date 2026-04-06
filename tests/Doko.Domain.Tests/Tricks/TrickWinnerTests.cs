using Doko.Domain.Tests.Helpers;

namespace Doko.Domain.Tests.Tricks;

public class TrickWinnerTests
{
    private static readonly ITrumpEvaluator Trump = NormalTrumpEvaluator.Instance;
    private const DulleRule SecondBeatsFirst = DulleRule.SecondBeatsFirst;
    private const DulleRule FirstBeatsSecond = DulleRule.FirstBeatsSecond;

    // ── Plain-suit tricks ─────────────────────────────────────────────────────

    [Fact]
    public void LedCardWins_WhenAllPlaySamePlainSuit()
    {
        var trick = B.Trick(
            (0, Suit.Kreuz, Rank.Ass, B.P0), // highest plain ♣
            (1, Suit.Kreuz, Rank.Zehn, B.P1),
            (2, Suit.Kreuz, Rank.Koenig, B.P2),
            (3, Suit.Kreuz, Rank.Neun, B.P3)
        );
        trick.Winner(Trump, SecondBeatsFirst).Should().Be(B.P0);
    }

    [Fact]
    public void HigherPlainCardBeatsLower_SameSuit()
    {
        var trick = B.Trick(
            (0, Suit.Kreuz, Rank.Koenig, B.P0),
            (1, Suit.Kreuz, Rank.Ass, B.P1), // ♣A beats ♣K
            (2, Suit.Kreuz, Rank.Zehn, B.P2),
            (3, Suit.Kreuz, Rank.Neun, B.P3)
        );
        trick.Winner(Trump, SecondBeatsFirst).Should().Be(B.P1);
    }

    [Fact]
    public void OffSuitPlainCard_CannotWin()
    {
        // P0 leads ♣A; P1 plays ♠A (different plain suit — cannot beat ♣ lead)
        var trick = B.Trick(
            (0, Suit.Kreuz, Rank.Ass, B.P0),
            (1, Suit.Pik, Rank.Ass, B.P1),
            (2, Suit.Kreuz, Rank.Neun, B.P2),
            (3, Suit.Kreuz, Rank.Neun, B.P3)
        );
        trick.Winner(Trump, SecondBeatsFirst).Should().Be(B.P0);
    }

    // ── Trump vs. plain ───────────────────────────────────────────────────────

    [Fact]
    public void TrumpBeatsPlainLead()
    {
        var trick = B.Trick(
            (0, Suit.Kreuz, Rank.Ass, B.P0), // plain ♣A leads
            (1, Suit.Karo, Rank.Neun, B.P1), // lowest trump beats it
            (2, Suit.Kreuz, Rank.Zehn, B.P2),
            (3, Suit.Kreuz, Rank.Koenig, B.P3)
        );
        trick.Winner(Trump, SecondBeatsFirst).Should().Be(B.P1);
    }

    [Fact]
    public void HigherTrumpBeatsLowerTrump()
    {
        var trick = B.Trick(
            (0, Suit.Karo, Rank.Neun, B.P0), // ♦9 (lowest trump) leads
            (1, Suit.Herz, Rank.Zehn, B.P1), // Dulle (highest trump)
            (2, Suit.Karo, Rank.Ass, B.P2), // Fuchs
            (3, Suit.Kreuz, Rank.Bube, B.P3)
        ); // ♣J
        trick.Winner(Trump, SecondBeatsFirst).Should().Be(B.P1); // Dulle wins
    }

    // ── Dulle tie-break ───────────────────────────────────────────────────────

    [Fact]
    public void TwoDullen_SecondBeatsFirst_Rule()
    {
        var trick = B.Trick(
            (0, Suit.Herz, Rank.Zehn, B.P0), // first Dulle
            (1, Suit.Herz, Rank.Zehn, B.P1), // second Dulle beats it
            (2, Suit.Karo, Rank.Neun, B.P2),
            (3, Suit.Karo, Rank.Neun, B.P3)
        );
        trick.Winner(Trump, SecondBeatsFirst).Should().Be(B.P1);
    }

    [Fact]
    public void TwoDullen_FirstBeatsSecond_Rule()
    {
        var trick = B.Trick(
            (0, Suit.Herz, Rank.Zehn, B.P0), // first Dulle
            (1, Suit.Herz, Rank.Zehn, B.P1), // second Dulle — does NOT win
            (2, Suit.Karo, Rank.Neun, B.P2),
            (3, Suit.Karo, Rank.Neun, B.P3)
        );
        trick.Winner(Trump, FirstBeatsSecond).Should().Be(B.P0);
    }

    // ── Equal non-Dulle trumps ────────────────────────────────────────────────

    [Fact]
    public void EqualNonDulleTrumps_FirstPlayedWins()
    {
        var trick = B.Trick(
            (0, Suit.Karo, Rank.Ass, B.P0), // first ♦A (Fuchs)
            (1, Suit.Karo, Rank.Ass, B.P1), // second ♦A — does NOT win
            (2, Suit.Karo, Rank.Neun, B.P2),
            (3, Suit.Karo, Rank.Neun, B.P3)
        );
        trick.Winner(Trump, SecondBeatsFirst).Should().Be(B.P0);
    }

    // ── Plain-suit lead, no follower ─────────────────────────────────────────

    [Fact]
    public void LedSuit_FirstCardStillWins_WhenNobodyFollows()
    {
        // Everyone plays off-suit; led card wins since nothing beats it
        var trick = B.Trick(
            (0, Suit.Kreuz, Rank.Ass, B.P0),
            (1, Suit.Pik, Rank.Ass, B.P1),
            (2, Suit.Herz, Rank.Koenig, B.P2),
            (3, Suit.Pik, Rank.Koenig, B.P3)
        );
        trick.Winner(Trump, SecondBeatsFirst).Should().Be(B.P0);
    }
}
