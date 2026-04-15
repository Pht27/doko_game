using Doko.Api.DTOs.Responses;
using Doko.Api.Services;
using Doko.Domain.Players;
using Microsoft.AspNetCore.Mvc;

namespace Doko.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(ITokenService tokenService) : ControllerBase
{
    [HttpPost("token")]
    public IActionResult Token([FromBody] TokenRequest req)
    {
        if (req.PlayerId < 0 || req.PlayerId > 3)
            return BadRequest(new ErrorResponse("invalid_player_id"));

        var token = tokenService.GenerateToken(new PlayerId((byte)req.PlayerId));
        return Ok(new { token });
    }
}

public record TokenRequest(int PlayerId);
