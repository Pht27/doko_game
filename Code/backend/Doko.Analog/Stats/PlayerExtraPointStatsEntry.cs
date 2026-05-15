namespace Doko.Analog.Stats;

public class PlayerExtraPointStatsEntry
{
    public int PlayerId { get; set; }
    public int ExtraPointId { get; set; }
    public string ExtraPointName { get; set; } = "";
    public int Party { get; set; }
    public int Occurrences { get; set; }
    public int TotalCount { get; set; }
    public int Wins { get; set; }
    public decimal WinRate { get; set; }
    public decimal AvgGameValue { get; set; }
}
