namespace Doko.Domain.Cards;

public record CardType(Suit Suit, Rank Rank)
{
    public bool IsDulle() => Suit == Suit.Herz && Rank == Rank.Zehn;
}
