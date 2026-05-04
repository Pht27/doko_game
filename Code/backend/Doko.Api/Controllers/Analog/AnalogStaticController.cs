using Doko.Analog.Services;
using Doko.Api.DTOs.Analog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doko.Api.Controllers.Analog;

[ApiController]
[Route("analog/static")]
[AllowAnonymous]
public class AnalogStaticController(AnalogStaticService staticService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStatic(CancellationToken ct)
    {
        var data = await staticService.GetStaticDataAsync(ct);
        return Ok(
            new StaticDataDto(
                data.GameModes.Select(g => new GameModeDto(g.Id, g.Name, g.IsSolo)).ToList(),
                data.SpecialCards.Select(s => new SpecialCardDto(s.Id, s.Name)).ToList(),
                data.ExtraPoints.Select(e => new ExtraPointDto(e.Id, e.Name)).ToList()
            )
        );
    }
}
