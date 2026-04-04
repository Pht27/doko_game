namespace Doko.Domain.Cards;

public static class CardPoints
{
    public static int Of(Rank rank) => rank switch
    {
        Rank.Ass    => 11,
        Rank.Zehn   => 10,
        Rank.Koenig => 4,
        Rank.Dame   => 3,
        Rank.Bube   => 2,
        Rank.Neun   => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null),
    };
}
