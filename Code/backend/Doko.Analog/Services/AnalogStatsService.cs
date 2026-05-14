using Doko.Analog.Stats;
using Microsoft.EntityFrameworkCore;

namespace Doko.Analog.Services;

public class AnalogStatsService(AnalogDbContext db)
{
    public async Task<PlayerStatsEntry?> GetPlayerStatsAsync(
        int playerId,
        CancellationToken ct = default
    ) => await db.PlayerStats.AsNoTracking().FirstOrDefaultAsync(e => e.PlayerId == playerId, ct);

    public async Task<IReadOnlyList<PlayerGameModeStatsEntry>> GetPlayerGameModeStatsAsync(
        int playerId,
        CancellationToken ct = default
    ) =>
        await db
            .PlayerGameModeStats.AsNoTracking()
            .Where(e => e.PlayerId == playerId)
            .OrderBy(e => e.GameModeName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PlayerSpecialCardStatsEntry>> GetPlayerSpecialCardStatsAsync(
        int playerId,
        CancellationToken ct = default
    ) =>
        await db
            .PlayerSpecialCardStats.AsNoTracking()
            .Where(e => e.PlayerId == playerId)
            .OrderByDescending(e => e.Occurrences)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PlayerExtraPointStatsEntry>> GetPlayerExtraPointStatsAsync(
        int playerId,
        CancellationToken ct = default
    ) =>
        await db
            .PlayerExtraPointStats.AsNoTracking()
            .Where(e => e.PlayerId == playerId)
            .OrderByDescending(e => e.Occurrences)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PlayerPartnerStatsEntry>> GetPlayerPartnerStatsAsync(
        int playerId,
        CancellationToken ct = default
    ) =>
        await db
            .PlayerPartnerStats.AsNoTracking()
            .Where(e => e.PlayerId == playerId)
            .OrderByDescending(e => e.GamesTogether)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<GameModeStatsEntry>> GetGameModeStatsAsync(
        CancellationToken ct = default
    ) =>
        await db.GameModeStats.AsNoTracking().OrderByDescending(e => e.TotalRounds).ToListAsync(ct);

    public async Task<IReadOnlyList<SpecialCardStatsEntry>> GetSpecialCardStatsAsync(
        CancellationToken ct = default
    ) =>
        await db
            .SpecialCardStats.AsNoTracking()
            .OrderByDescending(e => e.Occurrences)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ExtraPointStatsEntry>> GetExtraPointStatsAsync(
        CancellationToken ct = default
    ) =>
        await db
            .ExtraPointStats.AsNoTracking()
            .OrderByDescending(e => e.Occurrences)
            .ToListAsync(ct);
}
