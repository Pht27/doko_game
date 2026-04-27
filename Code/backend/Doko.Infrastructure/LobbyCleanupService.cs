using Doko.Application.Lobbies;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Doko.Infrastructure;

public sealed class LobbyCleanupService(
    ILobbyRepository lobbyRepository,
    IOptions<LobbyCleanupOptions> options,
    ILogger<LobbyCleanupService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(options.Value.CheckIntervalMinutes);
        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CleanupInactiveLobbiesAsync(stoppingToken);
        }
    }

    private async Task CleanupInactiveLobbiesAsync(CancellationToken ct)
    {
        var cutoff =
            DateTimeOffset.UtcNow - TimeSpan.FromHours(options.Value.InactivityThresholdHours);
        var lobbies = await lobbyRepository.GetAllAsync(ct);
        var stale = lobbies.Where(l => l.LastActivityAt < cutoff).ToList();

        foreach (var lobby in stale)
            await lobbyRepository.DeleteAsync(lobby.Id, ct);

        if (stale.Count > 0)
            logger.LogInformation(
                "Deleted {Count} inactive lobby/lobbies (inactive since {Cutoff})",
                stale.Count,
                cutoff
            );
    }
}
