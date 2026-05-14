using Doko.Analog.Services;
using Doko.Api.DTOs.Analog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doko.Api.Controllers.Analog;

[ApiController]
[AllowAnonymous]
public class AnalogStatsController(AnalogStatsService statsService) : ControllerBase
{
    [HttpGet("analog/players/{id:int}/stats")]
    public async Task<IActionResult> GetPlayerStats(int id, CancellationToken ct)
    {
        var entry = await statsService.GetPlayerStatsAsync(id, ct);
        if (entry is null)
            return NotFound();

        return Ok(
            new PlayerStatsDto(
                entry.PlayerId,
                entry.Name,
                entry.IsActive,
                entry.TotalGames,
                entry.TotalWins,
                entry.TotalWinRate,
                entry.TotalAvgGameValue,
                entry.TotalAvgPointsWonLost,
                entry.TotalAvgPointsEarned,
                entry.SoloGames,
                entry.SoloWins,
                entry.SoloWinRate,
                entry.SoloAvgGameValue,
                entry.SoloAvgPointsWonLost,
                entry.AloneGames,
                entry.AloneWins,
                entry.AloneWinRate,
                entry.AloneAvgPointsEarned
            )
        );
    }

    [HttpGet("analog/players/{id:int}/stats/game-modes")]
    public async Task<IActionResult> GetPlayerGameModeStats(int id, CancellationToken ct)
    {
        var entries = await statsService.GetPlayerGameModeStatsAsync(id, ct);
        return Ok(
            entries.Select(e => new PlayerGameModeStatsDto(
                e.GameModeId,
                e.GameModeName,
                e.Party,
                e.Games,
                e.Wins,
                e.WinRate,
                e.AvgGameValue,
                e.AvgPointsWonLost,
                e.AvgPointsEarned
            ))
        );
    }

    [HttpGet("analog/players/{id:int}/stats/special-cards")]
    public async Task<IActionResult> GetPlayerSpecialCardStats(int id, CancellationToken ct)
    {
        var entries = await statsService.GetPlayerSpecialCardStatsAsync(id, ct);
        return Ok(
            entries.Select(e => new PlayerSpecialCardStatsDto(
                e.SpecialCardId,
                e.SpecialCardName,
                e.Occurrences,
                e.Wins,
                e.WinRate,
                e.AvgGameValue,
                e.AvgPointsWonLost
            ))
        );
    }

    [HttpGet("analog/players/{id:int}/stats/extra-points")]
    public async Task<IActionResult> GetPlayerExtraPointStats(int id, CancellationToken ct)
    {
        var entries = await statsService.GetPlayerExtraPointStatsAsync(id, ct);
        return Ok(
            entries.Select(e => new PlayerExtraPointStatsDto(
                e.ExtraPointId,
                e.ExtraPointName,
                e.Occurrences,
                e.TotalCount,
                e.Wins,
                e.WinRate,
                e.AvgGameValue
            ))
        );
    }

    [HttpGet("analog/players/{id:int}/stats/partners")]
    public async Task<IActionResult> GetPlayerPartnerStats(int id, CancellationToken ct)
    {
        var entries = await statsService.GetPlayerPartnerStatsAsync(id, ct);
        return Ok(
            entries.Select(e => new PlayerPartnerStatsDto(
                e.PartnerId,
                e.PartnerName,
                e.GamesTogether,
                e.WinsTogether,
                e.WinRateTogether
            ))
        );
    }

    [HttpGet("analog/stats/game-modes")]
    public async Task<IActionResult> GetGameModeStats(CancellationToken ct)
    {
        var entries = await statsService.GetGameModeStatsAsync(ct);
        return Ok(
            entries.Select(e => new GameModeStatsDto(
                e.GameModeId,
                e.GameModeName,
                e.TotalRounds,
                e.AvgGameValue,
                e.ReWinRate,
                e.ReAvgGameValue
            ))
        );
    }

    [HttpGet("analog/stats/special-cards")]
    public async Task<IActionResult> GetSpecialCardStats(CancellationToken ct)
    {
        var entries = await statsService.GetSpecialCardStatsAsync(ct);
        return Ok(
            entries.Select(e => new SpecialCardStatsDto(
                e.SpecialCardId,
                e.Name,
                e.Occurrences,
                e.Wins,
                e.WinRate,
                e.AvgGameValue,
                e.AvgPointsWonLost
            ))
        );
    }

    [HttpGet("analog/stats/extra-points")]
    public async Task<IActionResult> GetExtraPointStats(CancellationToken ct)
    {
        var entries = await statsService.GetExtraPointStatsAsync(ct);
        return Ok(
            entries.Select(e => new ExtraPointStatsDto(
                e.ExtraPointId,
                e.Name,
                e.Occurrences,
                e.TotalCount,
                e.Wins,
                e.WinRate,
                e.AvgGameValue,
                e.AvgPointsWonLost
            ))
        );
    }
}
