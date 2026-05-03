namespace Doko.Analog.Entities;

public class AnalogTeamMember
{
    public int TeamId { get; set; }
    public AnalogTeam Team { get; set; } = null!;
    public int PlayerId { get; set; }
    public AnalogPlayer Player { get; set; } = null!;
    public int Position { get; set; }
}
