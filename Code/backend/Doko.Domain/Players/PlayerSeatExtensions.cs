using Doko.Domain.GameFlow;

namespace Doko.Domain.Players;

public static class PlayerSeatExtensions
{
    public static PlayerSeat Next(this PlayerSeat seat, PlayDirection direction)
    {
        int index = (int)seat;
        int nextIndex =
            direction == PlayDirection.Counterclockwise ? (index + 1) % 4 : (index + 3) % 4;
        return (PlayerSeat)nextIndex;
    }
}
