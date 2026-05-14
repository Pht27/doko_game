namespace Doko.Analog.Stats;

public class SpecialCardStatsEntry
{
    public int SpecialCardId { get; set; }
    public string Name { get; set; } = "";
    public int Occurrences { get; set; }
    public int Wins { get; set; }
    public decimal WinRate { get; set; }
    public decimal AvgGameValue { get; set; }
    public decimal AvgPointsWonLost { get; set; }
}
