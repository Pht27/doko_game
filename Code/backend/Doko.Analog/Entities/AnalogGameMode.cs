namespace Doko.Analog.Entities;

public class AnalogGameMode
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsSolo { get; set; }
    public Party? SoloParty { get; set; }
}
