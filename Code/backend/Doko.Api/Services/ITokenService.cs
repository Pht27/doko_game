using Doko.Domain.Players;

namespace Doko.Api.Services;

public interface ITokenService
{
    string GenerateToken(PlayerId playerId);
}
