using Doko.Domain.Players;
using Doko.Domain.Tests.Helpers;

namespace Doko.Domain.Tests.Players;

public class PlayerSeatExtensionsTests
{
    [Fact]
    public void Next_Counterclockwise_AdvancesBySeat()
    {
        B.P0.Next(PlayDirection.Counterclockwise).Should().Be(B.P1);
        B.P1.Next(PlayDirection.Counterclockwise).Should().Be(B.P2);
        B.P2.Next(PlayDirection.Counterclockwise).Should().Be(B.P3);
    }

    [Fact]
    public void Next_Counterclockwise_WrapsFromLastToFirst()
    {
        B.P3.Next(PlayDirection.Counterclockwise).Should().Be(B.P0);
    }

    [Fact]
    public void Next_Clockwise_DecreasesSeat()
    {
        B.P3.Next(PlayDirection.Clockwise).Should().Be(B.P2);
        B.P2.Next(PlayDirection.Clockwise).Should().Be(B.P1);
        B.P1.Next(PlayDirection.Clockwise).Should().Be(B.P0);
    }

    [Fact]
    public void Next_Clockwise_WrapsFromFirstToLast()
    {
        B.P0.Next(PlayDirection.Clockwise).Should().Be(B.P3);
    }
}
