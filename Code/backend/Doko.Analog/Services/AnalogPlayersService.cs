using Doko.Analog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Doko.Analog.Services;

public class AnalogPlayersService(AnalogDbContext db)
{
    public async Task<IReadOnlyList<PlayerStats>> GetPlayersAsync(CancellationToken ct = default)
    {
        var players = await db.Players.AsNoTracking().ToListAsync(ct);

        var roundStats = await db
            .TeamMembers.AsNoTracking()
            .Select(tm => new
            {
                tm.PlayerId,
                Won = tm.Team.Party == tm.Team.Round.WinningParty,
                tm.Team.Round.Points,
            })
            .GroupBy(x => x.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                GamesPlayed = g.Count(),
                Wins = g.Count(x => x.Won),
                Losses = g.Count(x => !x.Won),
                RoundPoints = g.Sum(x => x.Won ? (decimal)x.Points : -(decimal)x.Points),
            })
            .ToListAsync(ct);

        var statsMap = roundStats.ToDictionary(s => s.PlayerId);

        return players
            .Select(p =>
            {
                statsMap.TryGetValue(p.Id, out var s);
                return new PlayerStats(
                    p.Id,
                    p.Name,
                    p.IsActive,
                    p.StartingPoints + (s?.RoundPoints ?? 0),
                    s?.GamesPlayed ?? 0,
                    s?.Wins ?? 0,
                    s?.Losses ?? 0
                );
            })
            .ToList();
    }

    public async Task<PlayerDetail?> GetPlayerAsync(int id, CancellationToken ct = default)
    {
        var player = await db.Players.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (player is null)
            return null;

        var rounds = await db
            .TeamMembers.AsNoTracking()
            .Where(tm => tm.PlayerId == id)
            .Select(tm => new
            {
                tm.Team.Round.Id,
                tm.Team.Round.PlayedAt,
                tm.Team.Round.Points,
                Won = tm.Team.Party == tm.Team.Round.WinningParty,
            })
            .OrderBy(r => r.PlayedAt)
            .ToListAsync(ct);

        var cumulativePoints = player.StartingPoints;
        var recentRounds = rounds
            .TakeLast(50)
            .Select(r =>
            {
                var delta = r.Won ? (decimal)r.Points : -(decimal)r.Points;
                cumulativePoints += delta;
                return new PlayerRoundEntry(r.Id, r.PlayedAt, delta, r.Won, cumulativePoints);
            })
            .ToList();

        var totalPoints =
            player.StartingPoints + rounds.Sum(r => r.Won ? (decimal)r.Points : -(decimal)r.Points);

        return new PlayerDetail(
            player.Id,
            player.Name,
            player.IsActive,
            totalPoints,
            rounds.Count,
            rounds.Count(r => r.Won),
            rounds.Count(r => !r.Won),
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

    public async Task<AnalogPlayer?> SetPlayerActiveAsync(
        int id,
        bool isActive,
        CancellationToken ct = default
    )
    {
        var player = await db.Players.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (player is null)
            return null;

        player.IsActive = isActive;
        await db.SaveChangesAsync(ct);
        return player;
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default) =>
        await db.Players.AnyAsync(p => p.Name == name, ct);
}

public record PlayerStats(
    int Id,
    string Name,
    bool IsActive,
    decimal TotalPoints,
    int GamesPlayed,
    int Wins,
    int Losses
);

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
