namespace Doko.Analog.Entities;

public class AnalogTeam
{
    public int Id { get; set; }
    public int RoundId { get; set; }
    public AnalogRound Round { get; set; } = null!;
    public string? Name { get; set; }
    public Party Party { get; set; }
    public ICollection<AnalogTeamMember> Members { get; set; } = [];
}
