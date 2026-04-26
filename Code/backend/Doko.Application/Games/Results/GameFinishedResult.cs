using Doko.Domain.Parties;
using Doko.Domain.Scoring;

namespace Doko.Application.Games.Results;

public record GameFinishedResult(
    GameResult Result,
    IReadOnlyList<int> NetPointsPerSeat,
    IReadOnlyList<Party?> PartyPerSeat,
    /// <summary>
    /// True when the VorbehaltRauskommer should advance to the next seat for the next game.
    /// False for Soli, Armut, and SchlankerMartin — the same seat leads again.
    /// Schmeißen is handled separately (dedicated endpoint skips advancement).
    /// </summary>
    bool ShouldAdvanceRauskommer,
    /// <summary>Null for Normalspiel; the reservation priority name otherwise (e.g. "Damensolo").</summary>
    string? GameMode = null
);
