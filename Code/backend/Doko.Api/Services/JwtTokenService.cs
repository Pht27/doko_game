using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Doko.Domain.Players;
using Microsoft.IdentityModel.Tokens;

namespace Doko.Api.Services;

public sealed class JwtTokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateToken(PlayerSeat seat)
    {
        var key =
            configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("seat_index", ((int)seat).ToString())]),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}
