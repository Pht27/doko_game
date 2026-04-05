namespace Doko.Application.Common;

public abstract record GameActionResult<T>
{
    public sealed record Ok(T Value) : GameActionResult<T>;
    public sealed record Failure(GameError Error) : GameActionResult<T>;
}
