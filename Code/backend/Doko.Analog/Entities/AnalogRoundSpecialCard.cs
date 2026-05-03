namespace Doko.Analog.Entities;

public class AnalogRoundSpecialCard
{
    public int RoundId { get; set; }
    public AnalogRound Round { get; set; } = null!;
    public int SpecialCardId { get; set; }
    public AnalogSpecialCard SpecialCard { get; set; } = null!;
    public int TeamId { get; set; }
    public AnalogTeam Team { get; set; } = null!;
}
