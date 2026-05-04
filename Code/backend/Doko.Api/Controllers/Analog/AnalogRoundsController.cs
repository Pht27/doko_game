using Doko.Analog;
using Doko.Analog.Services;
using Doko.Api.DTOs.Analog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doko.Api.Controllers.Analog;

[ApiController]
[Route("analog/rounds")]
[AllowAnonymous]
public class AnalogRoundsController(AnalogRoundsService roundsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRounds(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    )
    {
        var result = await roundsService.GetRoundsAsync(page, pageSize, ct);
        return Ok(
            new RoundListResponse(
                result.Total,
                result.Page,
                result
                    .Items.Select(r => new RoundListItemDto(
                        r.Id,
                        r.PlayedAt,
                        r.WinningParty,
                        r.Points,
                        r.GameMode,
                        r.RePlayers.Select(p => new PlayerInfoDto(p.Id, p.Name)).ToArray(),
                        r.KontraPlayers.Select(p => new PlayerInfoDto(p.Id, p.Name)).ToArray(),
                        r.Comment
                    ))
                    .ToArray()
            )
        );
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRound(int id, CancellationToken ct)
    {
        var round = await roundsService.GetRoundAsync(id, ct);
        if (round is null)
            return NotFound();

        return Ok(
            new RoundDetailDto(
                round.Id,
                round.PlayedAt,
                round.WinningParty,
                round.Points,
                new GameModeInfoDto(round.GameMode.Id, round.GameMode.Name, round.GameMode.IsSolo),
                round
                    .Teams.Select(t => new RoundTeamDetailDto(
                        t.Party,
                        t.Players.Select(p => new PlayerInfoDto(p.Id, p.Name)).ToArray(),
                        t.SpecialCards.Select(s => new SpecialCardInfoDto(s.Id, s.Name)).ToArray(),
                        t.ExtraPoints.Select(e => new ExtraPointEntryDto(e.Id, e.Name, e.Count))
                            .ToArray()
                    ))
                    .ToArray(),
                round.Comment
            )
        );
    }

    [HttpPost]
    public async Task<IActionResult> CreateRound([FromBody] RoundRequest body, CancellationToken ct)
    {
        if (ValidationError(body) is { } err)
            return BadRequest(new { error = err });

        var input = ToInput(body);
        var round = await roundsService.CreateRoundAsync(input, ct);

        return CreatedAtAction(nameof(GetRound), new { id = round.Id }, new { round.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateRound(
        int id,
        [FromBody] RoundRequest body,
        CancellationToken ct
    )
    {
        if (ValidationError(body) is { } err)
            return BadRequest(new { error = err });

        var input = ToInput(body);
        var round = await roundsService.UpdateRoundAsync(id, input, ct);
        if (round is null)
            return NotFound();

        return Ok(new { round.Id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRound(int id, CancellationToken ct)
    {
        var deleted = await roundsService.DeleteRoundAsync(id, ct);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private static string? ValidationError(RoundRequest body)
    {
        if (body.Teams.Length != 4)
            return "exactly_four_teams_required";

        var hasRe = body.Teams.Any(t => t.Party == Party.Re);
        var hasKontra = body.Teams.Any(t => t.Party == Party.Kontra);
        if (!hasRe || !hasKontra)
            return "both_parties_required";

        if (body.Teams.Any(t => t.PlayerIds.Length < 1 || t.PlayerIds.Length > 2))
            return "team_size_invalid";

        var allPlayerIds = body.Teams.SelectMany(t => t.PlayerIds).ToArray();
        if (allPlayerIds.Distinct().Count() != allPlayerIds.Length)
            return "duplicate_player";

        return null;
    }

    private static RoundInput ToInput(RoundRequest body) =>
        new(
            body.PlayedAt,
            body.WinningParty,
            body.Points,
            body.GameModeId,
            body.Teams.Select(t => new TeamInput(
                    t.Party,
                    t.PlayerIds,
                    t.SpecialCardIds,
                    t.ExtraPoints.Select(ep => new ExtraPointInput(ep.ExtraPointId, ep.Count))
                        .ToArray()
                ))
                .ToArray(),
            body.Comment
        );
}
