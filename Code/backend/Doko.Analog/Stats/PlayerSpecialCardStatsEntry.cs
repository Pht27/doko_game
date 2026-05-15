namespace Doko.Analog.Stats;

public class PlayerSpecialCardStatsEntry
{
    public int PlayerId { get; set; }
    public int SpecialCardId { get; set; }
    public string SpecialCardName { get; set; } = "";
    public int Party { get; set; }
    public int Occurrences { get; set; }
    public int Wins { get; set; }
    public decimal WinRate { get; set; }
    public decimal AvgGameValue { get; set; }
    public decimal AvgPointsWonLost { get; set; }
}
