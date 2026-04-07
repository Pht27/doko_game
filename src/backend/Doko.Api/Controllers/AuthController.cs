using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Doko.Api.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Doko.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IConfiguration configuration) : ControllerBase
{
    [HttpPost("token")]
    public IActionResult Token([FromBody] TokenRequest req)
    {
        if (req.PlayerId < 0 || req.PlayerId > 3)
            return BadRequest(new ErrorResponse("invalid_player_id"));

        var key =
            configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("player_id", req.PlayerId.ToString())]),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return Ok(new { token = handler.WriteToken(token) });
    }
}

public record TokenRequest(int PlayerId);
