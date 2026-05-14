namespace Doko.Analog.Stats;

public class PlayerGameModeStatsEntry
{
    public int PlayerId { get; set; }
    public int GameModeId { get; set; }
    public string GameModeName { get; set; } = "";
    public int Party { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
    public decimal WinRate { get; set; }
    public decimal AvgGameValue { get; set; }
    public decimal AvgPointsWonLost { get; set; }
    public decimal AvgPointsEarned { get; set; }
}
