using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Doko.Analog.IntegrationTests;

[Collection("Analog")]
public class AnalogPlayersTests(AnalogTestFixture fixture)
{
    private static string UniqueName() => $"Spieler_{Guid.NewGuid():N}"[..20];

    [Fact]
    public async Task GetPlayers_ReturnsOk()
    {
        var client = fixture.CreateClient();
        var response = await client.GetAsync("/analog/players");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreatePlayer_ValidRequest_Returns201WithPlayer()
    {
        var client = fixture.CreateClient();
        var name = UniqueName();

        var response = await client.PostAsJsonAsync(
            "/analog/players",
            new { Name = name, StartingPoints = 10.5m }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be(name);
        body.GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CreatePlayer_DuplicateName_Returns409WithError()
    {
        var client = fixture.CreateClient();
        var name = UniqueName();

        await client.PostAsJsonAsync("/analog/players", new { Name = name });
        var response = await client.PostAsJsonAsync("/analog/players", new { Name = name });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("error").GetString().Should().Be("name_taken");
    }

    [Fact]
    public async Task GetPlayer_UnknownId_Returns404()
    {
        var client = fixture.CreateClient();
        var response = await client.GetAsync("/analog/players/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPlayer_ExistingId_ReturnsDetail()
    {
        var client = fixture.CreateClient();
        var name = UniqueName();

        var createResponse = await client.PostAsJsonAsync("/analog/players", new { Name = name });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetInt32();

        var response = await client.GetAsync($"/analog/players/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetInt32().Should().Be(id);
        body.GetProperty("name").GetString().Should().Be(name);
        body.GetProperty("recentRounds").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task PatchPlayer_SetInactive_Returns200()
    {
        var client = fixture.CreateClient();
        var name = UniqueName();

        var createResponse = await client.PostAsJsonAsync("/analog/players", new { Name = name });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetInt32();

        var response = await client.PatchAsJsonAsync(
            $"/analog/players/{id}",
            new { IsActive = false }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isActive").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task PatchPlayer_UnknownId_Returns404()
    {
        var client = fixture.CreateClient();
        var response = await client.PatchAsJsonAsync(
            "/analog/players/999999",
            new { IsActive = false }
        );
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
