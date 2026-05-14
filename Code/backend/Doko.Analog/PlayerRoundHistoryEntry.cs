namespace Doko.Analog;

public class PlayerRoundHistoryEntry
{
    public int PlayerId { get; set; }
    public int RoundId { get; set; }
    public DateTime PlayedAt { get; set; }
    public decimal PointDelta { get; set; }
    public bool Won { get; set; }
    public decimal CumulativePoints { get; set; }
}
