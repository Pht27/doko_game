namespace Doko.Domain.Tests.Cards;

public class CardTypeTests
{
    [Fact]
    public void IsDulle_ReturnsTrue_ForHerzZehn()
    {
        var card = new CardType(Suit.Herz, Rank.Zehn);
        card.IsDulle().Should().BeTrue();
    }

    [Theory]
    [InlineData(Suit.Karo, Rank.Zehn)]
    [InlineData(Suit.Pik, Rank.Zehn)]
    [InlineData(Suit.Kreuz, Rank.Zehn)]
    [InlineData(Suit.Herz, Rank.Ass)]
    [InlineData(Suit.Herz, Rank.Koenig)]
    [InlineData(Suit.Herz, Rank.Neun)]
    public void IsDulle_ReturnsFalse_ForOtherCards(Suit suit, Rank rank)
    {
        var card = new CardType(suit, rank);
        card.IsDulle().Should().BeFalse();
    }
}
