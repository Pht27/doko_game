namespace Doko.Analog.Entities;

public class AnalogRoundExtraPoint
{
    public int RoundId { get; set; }
    public AnalogRound Round { get; set; } = null!;
    public int ExtraPointId { get; set; }
    public AnalogExtraPoint ExtraPoint { get; set; } = null!;
    public int TeamId { get; set; }
    public AnalogTeam Team { get; set; } = null!;
    public int Count { get; set; }
}
