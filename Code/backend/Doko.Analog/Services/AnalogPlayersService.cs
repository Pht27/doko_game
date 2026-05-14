using Doko.Analog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Doko.Analog.Services;

public class AnalogPlayersService(AnalogDbContext db)
{
    public async Task<IReadOnlyList<PlayerLeaderboardEntry>> GetPlayersAsync(
        CancellationToken ct = default
    ) => await db.PlayerLeaderboard.AsNoTracking().ToListAsync(ct);

    public async Task<PlayerDetail?> GetPlayerAsync(int id, CancellationToken ct = default)
    {
        var player = await db.Players.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (player is null)
            return null;

        var history = await db
            .PlayerRoundHistory.AsNoTracking()
            .Where(r => r.PlayerId == id)
            .OrderBy(r => r.PlayedAt)
            .ToListAsync(ct);

        var recentRounds = history
            .Select(r => new PlayerRoundEntry(
                r.RoundId,
                r.PlayedAt,
                r.PointDelta,
                r.Won,
                r.CumulativePoints
            ))
            .ToList();

        var totalPoints = history.Count > 0 ? history[^1].CumulativePoints : player.StartingPoints;

        return new PlayerDetail(
            player.Id,
            player.Name,
            player.IsActive,
            totalPoints,
            history.Count,
            history.Count(r => r.Won),
            history.Count(r => !r.Won),
            recentRounds
        );
    }

    public async Task<AnalogPlayer> CreatePlayerAsync(
        string name,
        decimal startingPoints,
        CancellationToken ct = default
    )
    {
        var player = new AnalogPlayer
        {
            Name = name,
            StartingPoints = startingPoints,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        db.Players.Add(player);
        await db.SaveChangesAsync(ct);
        return player;
    }

    public async Task<AnalogPlayer?> PatchPlayerAsync(
        int id,
        bool isActive,
        string? name,
        CancellationToken ct = default
    )
    {
        var player = await db.Players.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (player is null)
            return null;

        player.IsActive = isActive;
        if (name is not null)
            player.Name = name;
        await db.SaveChangesAsync(ct);
        return player;
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default) =>
        await db.Players.AnyAsync(p => p.Name == name, ct);

    public async Task<bool> NameTakenByOtherAsync(
        int id,
        string name,
        CancellationToken ct = default
    ) => await db.Players.AnyAsync(p => p.Name == name && p.Id != id, ct);
}

public record PlayerRoundEntry(
    int RoundId,
    DateTime PlayedAt,
    decimal Points,
    bool Won,
    decimal CumulativePoints
);

public record PlayerDetail(
    int Id,
    string Name,
    bool IsActive,
    decimal TotalPoints,
    int GamesPlayed,
    int Wins,
    int Losses,
    IReadOnlyList<PlayerRoundEntry> RecentRounds
);
