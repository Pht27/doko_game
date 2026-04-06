namespace Doko.Application.Games.Results;

/// <param name="Accepted">True if the rich player accepted the Armut.</param>
/// <param name="SchwarzesSau">True if nobody accepted and the game continues as Schwarze Sau.</param>
public record AcceptArmutResult(bool Accepted, bool SchwarzesSau = false);
