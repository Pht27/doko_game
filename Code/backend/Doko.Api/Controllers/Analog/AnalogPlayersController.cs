using Doko.Analog.Services;
using Doko.Api.DTOs.Analog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doko.Api.Controllers.Analog;

[ApiController]
[Route("analog/players")]
[AllowAnonymous]
public class AnalogPlayersController(AnalogPlayersService playersService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPlayers(CancellationToken ct)
    {
        var players = await playersService.GetPlayersAsync(ct);
        var result = players.Select(p => new PlayerListItemDto(
            p.PlayerId,
            p.Name,
            p.IsActive,
            p.TotalPoints,
            p.GamesPlayed,
            p.Wins,
            p.Losses,
            p.WinRate,
            p.AvgPointsPerGame
        ));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPlayer(int id, CancellationToken ct)
    {
        var player = await playersService.GetPlayerAsync(id, ct);
        if (player is null)
            return NotFound();

        var rounds = player
            .RecentRounds.Select(r => new PlayerRoundDto(
                r.RoundId,
                r.PlayedAt,
                r.Points,
                r.Won,
                r.CumulativePoints
            ))
            .ToList();

        return Ok(
            new PlayerDetailDto(
                player.Id,
                player.Name,
                player.IsActive,
                player.TotalPoints,
                player.GamesPlayed,
                player.Wins,
                player.Losses,
                rounds
            )
        );
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlayer(
        [FromBody] CreatePlayerRequest body,
        CancellationToken ct
    )
    {
        if (await playersService.NameExistsAsync(body.Name, ct))
            return Conflict(new { error = "name_taken" });

        var player = await playersService.CreatePlayerAsync(body.Name, body.StartingPoints, ct);

        return CreatedAtAction(
            nameof(GetPlayer),
            new { id = player.Id },
            new PlayerListItemDto(
                player.Id,
                player.Name,
                player.IsActive,
                player.StartingPoints,
                0,
                0,
                0,
                0,
                0
            )
        );
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchPlayer(
        int id,
        [FromBody] PatchPlayerRequest body,
        CancellationToken ct
    )
    {
        if (body.Name is not null && await playersService.NameTakenByOtherAsync(id, body.Name, ct))
            return Conflict(new { error = "name_taken" });

        var player = await playersService.PatchPlayerAsync(id, body.IsActive, body.Name, ct);
        if (player is null)
            return NotFound();

        return Ok(
            new
            {
                player.Id,
                player.Name,
                player.IsActive,
            }
        );
    }
}
