using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Doko.Api.IntegrationTests.Stubs;
using Doko.Application.Games.UseCases;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Doko.Api.IntegrationTests;

/// <summary>Shared factory that replaces use cases with stubs and uses a fixed test JWT key.</summary>
public class ApiTestFixture : WebApplicationFactory<Program>
{
    public const string TestJwtKey = "test-jwt-key-for-integration-tests-32chars!!";

    public IStartGameUseCase StartGame { get; } = new StubStartGameUseCase();
    public IDealCardsUseCase DealCards { get; } = new StubDealCardsUseCase();
    public IMakeReservationUseCase MakeReservation { get; } = new StubMakeReservationUseCase();
    public IPlayCardUseCase PlayCard { get; } = new StubPlayCardUseCase();
    public IMakeAnnouncementUseCase MakeAnnouncement { get; } = new StubMakeAnnouncementUseCase();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", TestJwtKey);

        builder.ConfigureServices(services =>
        {
            services.Replace(ServiceDescriptor.Singleton<IStartGameUseCase>(StartGame));
            services.Replace(ServiceDescriptor.Singleton<IDealCardsUseCase>(DealCards));
            services.Replace(ServiceDescriptor.Singleton<IMakeReservationUseCase>(MakeReservation));
            services.Replace(ServiceDescriptor.Singleton<IPlayCardUseCase>(PlayCard));
            services.Replace(
                ServiceDescriptor.Singleton<IMakeAnnouncementUseCase>(MakeAnnouncement)
            );
        });
    }

    public string GenerateToken(int playerId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("player_id", playerId.ToString())]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        };
        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    public HttpClient CreateAuthenticatedClient(int playerId = 0)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                GenerateToken(playerId)
            );
        return client;
    }
}
