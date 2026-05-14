namespace Doko.Analog.Stats;

public class PlayerPartnerStatsEntry
{
    public int PlayerId { get; set; }
    public int PartnerId { get; set; }
    public string PartnerName { get; set; } = "";
    public int GamesTogether { get; set; }
    public int WinsTogether { get; set; }
    public decimal WinRateTogether { get; set; }
}
