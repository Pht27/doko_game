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
        if (req.SeatIndex < 0 || req.SeatIndex > 3)
            return BadRequest(new ErrorResponse("invalid_seat_index"));

        var token = tokenService.GenerateToken((PlayerSeat)req.SeatIndex);
        return Ok(new { token });
    }
}

public record TokenRequest(int SeatIndex);
