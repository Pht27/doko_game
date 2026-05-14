namespace Doko.Analog.Entities;

public class AnalogPlayer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public decimal StartingPoints { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public string? HeroCard { get; set; }
}
