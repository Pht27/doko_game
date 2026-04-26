namespace Doko.Application.Lobbies;

public abstract record LobbyActionResult<T>
{
    public sealed record Ok(T Value) : LobbyActionResult<T>;

    public sealed record Failure(LobbyError Error) : LobbyActionResult<T>;
}
