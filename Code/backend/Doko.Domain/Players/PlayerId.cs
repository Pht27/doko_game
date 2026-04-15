namespace Doko.Domain.Players;

/// <summary>Identifies one of the four players (0–3).</summary>
public readonly record struct PlayerId(byte Value)
{
    public override string ToString() => Value.ToString();
}
