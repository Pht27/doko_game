namespace Doko.Domain.GameFlow;

public enum GamePhase
{
    Dealing,
    Reservations,
    Playing,
    Scoring,
    Finished,

    /// <summary>A player declared Schmeißen and it won the reservation — game ends with no result.</summary>
    Geschmissen,
}
