namespace Doko.Domain.Cards;

/// <summary>Identifies one of the 48 physical cards (0–47). Two cards share a CardType but differ in CardId.</summary>
public readonly record struct CardId(byte Value)
{
    public override string ToString() => Value.ToString();
}
