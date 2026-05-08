namespace Doko.Analog;

public class PlayerLeaderboardEntry
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
    public decimal TotalPoints { get; set; }
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public decimal WinRate { get; set; }
    public decimal AvgPointsPerGame { get; set; }
}
