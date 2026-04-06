namespace Doko.Domain.Tests.Cards;

public class CardPointsTests
{
    [Theory]
    [InlineData(Rank.Ass, 11)]
    [InlineData(Rank.Zehn, 10)]
    [InlineData(Rank.Koenig, 4)]
    [InlineData(Rank.Dame, 3)]
    [InlineData(Rank.Bube, 2)]
    [InlineData(Rank.Neun, 0)]
    public void Of_ReturnsCorrectPoints(Rank rank, int expected) =>
        CardPoints.Of(rank).Should().Be(expected);

    [Fact]
    public void AllSixRanks_SumToExpectedPerSuit()
    {
        // A + 10 + K + Q + J + 9 = 11 + 10 + 4 + 3 + 2 + 0 = 30 per suit
        int sum = Enum.GetValues<Rank>().Sum(CardPoints.Of);
        sum.Should().Be(30);
    }
}
