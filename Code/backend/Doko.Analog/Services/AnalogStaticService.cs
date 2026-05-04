using Microsoft.EntityFrameworkCore;

namespace Doko.Analog.Services;

public class AnalogStaticService(AnalogDbContext db)
{
    public async Task<StaticData> GetStaticDataAsync(CancellationToken ct = default)
    {
        var gameModes = await db
            .GameModes.AsNoTracking()
            .Select(g => new GameModeItem(g.Id, g.Name, g.IsSolo))
            .ToListAsync(ct);

        var specialCards = await db
            .SpecialCards.AsNoTracking()
            .Select(s => new SpecialCardItem(s.Id, s.Name))
            .ToListAsync(ct);

        var extraPoints = await db
            .ExtraPoints.AsNoTracking()
            .Select(e => new ExtraPointItem(e.Id, e.Name))
            .ToListAsync(ct);

        return new StaticData(gameModes, specialCards, extraPoints);
    }
}

public record StaticData(
    IReadOnlyList<GameModeItem> GameModes,
    IReadOnlyList<SpecialCardItem> SpecialCards,
    IReadOnlyList<ExtraPointItem> ExtraPoints
);

public record GameModeItem(int Id, string Name, bool IsSolo);

public record SpecialCardItem(int Id, string Name);

public record ExtraPointItem(int Id, string Name);
