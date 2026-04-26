namespace Doko.Application.Games.Results;

/// <param name="FinishedResult">Non-null if the solo was chosen on the very last trick and the
/// game finished immediately after selection.</param>
public record ChooseSchwarzesSauSoloResult(GameFinishedResult? FinishedResult);
