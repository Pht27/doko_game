namespace Doko.Infrastructure;

public sealed class LobbyCleanupOptions
{
    public int InactivityThresholdHours { get; set; } = 2;
    public int CheckIntervalMinutes { get; set; } = 30;
}
