namespace Doko.Analog.Entities;

public class AnalogRound
{
    public int Id { get; set; }
    public Party WinningParty { get; set; }
    public int Points { get; set; }
    public DateTime PlayedAt { get; set; }
    public int GameModeId { get; set; }
    public AnalogGameMode GameMode { get; set; } = null!;
    public ICollection<AnalogTeam> Teams { get; set; } = [];
}
