using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Doko.Api.IntegrationTests;

public class AuthControllerTests(ApiTestFixture fixture) : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Token_ValidPlayerSeat_Returns200WithToken(int playerId)
    {
        var response = await _client.PostAsJsonAsync("/auth/token", new { seatIndex = playerId });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(100)]
    public async Task Token_InvalidPlayerSeat_Returns400(int playerId)
    {
        var response = await _client.PostAsJsonAsync("/auth/token", new { seatIndex = playerId });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
