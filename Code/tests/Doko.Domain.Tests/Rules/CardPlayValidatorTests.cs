using Doko.Domain.Tests.Helpers;

namespace Doko.Domain.Tests.Rules;

public class CardPlayValidatorTests
{
    private static readonly ITrumpEvaluator Trump = NormalTrumpEvaluator.Instance;

    private static Trick EmptyTrick() => new();

    private static Trick TrickLedWith(Suit suit, Rank rank)
    {
        var t = new Trick();
        t.Add(new TrickCard(B.Card(0, suit, rank), B.P0));
        return t;
    }

    // ── First card in trick ───────────────────────────────────────────────────

    [Fact]
    public void EmptyTrick_AnyCardIsLegal()
    {
        var hand = B.HandOf(B.Card(0, Suit.Kreuz, Rank.Ass));
        var card = B.Card(0, Suit.Kreuz, Rank.Ass);
        CardPlayValidator.CanPlay(card, hand, EmptyTrick(), Trump).Should().BeTrue();
    }

    // ── Trump lead ────────────────────────────────────────────────────────────

    [Fact]
    public void TrumpLed_MustPlayTrump_WhenHandHasTrump()
    {
        var trump = B.Card(0, Suit.Karo, Rank.Ass); // trump in hand
        var plain = B.Card(1, Suit.Kreuz, Rank.Ass); // plain in hand
        var hand = B.HandOf(trump, plain);
        var trick = TrickLedWith(Suit.Karo, Rank.Neun); // trump lead

        CardPlayValidator.CanPlay(plain, hand, trick, Trump).Should().BeFalse();
        CardPlayValidator.CanPlay(trump, hand, trick, Trump).Should().BeTrue();
    }

    [Fact]
    public void TrumpLed_AnyCardAllowed_WhenHandHasNoTrump()
    {
        var plain = B.Card(0, Suit.Kreuz, Rank.Ass);
        var hand = B.HandOf(plain);
        var trick = TrickLedWith(Suit.Karo, Rank.Neun); // trump lead

        CardPlayValidator.CanPlay(plain, hand, trick, Trump).Should().BeTrue();
    }

    // ── Plain-suit lead ───────────────────────────────────────────────────────

    [Fact]
    public void PlainLed_MustFollowSuit_WhenHandHasLedSuit()
    {
        var kreuzAss = B.Card(0, Suit.Kreuz, Rank.Ass);
        var kreuzKoenig = B.Card(1, Suit.Kreuz, Rank.Koenig);
        var pikAss = B.Card(2, Suit.Pik, Rank.Ass);
        var hand = B.HandOf(kreuzAss, kreuzKoenig, pikAss);
        var trick = TrickLedWith(Suit.Kreuz, Rank.Neun);

        CardPlayValidator.CanPlay(kreuzAss, hand, trick, Trump).Should().BeTrue();
        CardPlayValidator.CanPlay(kreuzKoenig, hand, trick, Trump).Should().BeTrue();
        CardPlayValidator.CanPlay(pikAss, hand, trick, Trump).Should().BeFalse();
    }

    [Fact]
    public void PlainLed_AnyCardAllowed_WhenHandMissingLedSuit()
    {
        var pikAss = B.Card(0, Suit.Pik, Rank.Ass);
        var trump = B.Card(1, Suit.Karo, Rank.Ass);
        var hand = B.HandOf(pikAss, trump);
        var trick = TrickLedWith(Suit.Kreuz, Rank.Neun); // ♣ lead, no ♣ in hand

        CardPlayValidator.CanPlay(pikAss, hand, trick, Trump).Should().BeTrue();
        CardPlayValidator.CanPlay(trump, hand, trick, Trump).Should().BeTrue();
    }

    // ── Dulle edge case ───────────────────────────────────────────────────────

    [Fact]
    public void HerzZehn_CountsAsTrump_NotAsPlainHerz()
    {
        // Plain ♥ is led. Dulle (♥10) is trump, not a plain Herz card.
        // Player has only Dulle in hand — they do NOT have plain Herz, so any card is legal.
        var dulle = B.Card(0, Suit.Herz, Rank.Zehn);
        var hand = B.HandOf(dulle);
        var trick = TrickLedWith(Suit.Herz, Rank.Ass); // plain ♥ lead

        // Dulle cannot satisfy plain ♥ obligation; player has no plain Herz → free to play anything
        CardPlayValidator.CanPlay(dulle, hand, trick, Trump).Should().BeTrue();
    }

    [Fact]
    public void HerzZehn_LedAsTrump_RequiresTrumpFromHand()
    {
        // Dulle is played as trump lead. Player must follow with trump if they have it.
        var dulle = B.Card(0, Suit.Herz, Rank.Zehn);
        var trump = B.Card(1, Suit.Karo, Rank.Ass);
        var plain = B.Card(2, Suit.Kreuz, Rank.Ass);
        var hand = B.HandOf(trump, plain);
        var trick = new Trick();
        trick.Add(new TrickCard(dulle, B.P0)); // Dulle leads

        CardPlayValidator.CanPlay(trump, hand, trick, Trump).Should().BeTrue();
        CardPlayValidator.CanPlay(plain, hand, trick, Trump).Should().BeFalse();
    }
}
