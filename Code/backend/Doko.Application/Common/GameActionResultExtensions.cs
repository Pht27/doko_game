namespace Doko.Application.Common;

public static class GameActionResultExtensions
{
    public static GameActionResult<T> Ok<T>(T value) => new GameActionResult<T>.Ok(value);

    public static GameActionResult<T> Fail<T>(GameError error) =>
        new GameActionResult<T>.Failure(error);
}
