namespace Doko.Analog.Entities;

public class AnalogComment
{
    public int Id { get; set; }
    public int RoundId { get; set; }
    public AnalogRound Round { get; set; } = null!;
    public string Text { get; set; } = "";
}
