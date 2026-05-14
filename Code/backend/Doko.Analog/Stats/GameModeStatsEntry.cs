namespace Doko.Analog.Stats;

public class GameModeStatsEntry
{
    public int GameModeId { get; set; }
    public string GameModeName { get; set; } = "";
    public int TotalRounds { get; set; }
    public decimal AvgGameValue { get; set; }
    public decimal? ReWinRate { get; set; }
    public decimal? ReAvgGameValue { get; set; }
}
