namespace Doko.Analog.Stats;

public class PlayerStatsEntry
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }

    public int TotalGames { get; set; }
    public int TotalWins { get; set; }
    public decimal TotalWinRate { get; set; }
    public decimal TotalAvgGameValue { get; set; }
    public decimal TotalAvgPointsWonLost { get; set; }
    public decimal TotalAvgPointsEarned { get; set; }

    public int SoloGames { get; set; }
    public int SoloWins { get; set; }
    public decimal SoloWinRate { get; set; }
    public decimal SoloAvgGameValue { get; set; }
    public decimal SoloAvgPointsWonLost { get; set; }

    public int AloneGames { get; set; }
    public int AloneWins { get; set; }
    public decimal AloneWinRate { get; set; }
    public decimal AloneAvgPointsEarned { get; set; }
}
