namespace Doko.Domain.Lobby;

/// <summary>Identifies a lobby (waiting room before a game starts).</summary>
public readonly record struct LobbyId(Guid Value)
{
    public static LobbyId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
