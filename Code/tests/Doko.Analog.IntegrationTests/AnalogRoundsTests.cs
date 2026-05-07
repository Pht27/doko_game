using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Doko.Analog.IntegrationTests;

[Collection("Analog")]
public class AnalogRoundsTests(AnalogTestFixture fixture)
{
    private static string UniqueName() => $"R_{Guid.NewGuid():N}"[..20];

    private async Task<int> CreatePlayerAsync(HttpClient client, string? name = null)
    {
        var response = await client.PostAsJsonAsync(
            "/analog/players",
            new { Name = name ?? UniqueName() }
        );
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetInt32();
    }

    // Validation requires exactly 4 teams, each with 1-2 players (one slot per player)
    private static object BuildRoundBody(
        int re1,
        int re2,
        int kontra1,
        int kontra2,
        string? comment = null
    ) =>
        new
        {
            PlayedAt = DateTime.UtcNow,
            WinningParty = 0, // Re
            Points = 2,
            GameModeId = 3, // Normal
            Teams = new object[]
            {
                new
                {
                    Party = 0,
                    PlayerIds = new[] { re1 },
                    SpecialCardIds = Array.Empty<int>(),
                    ExtraPoints = Array.Empty<object>(),
                },
                new
                {
                    Party = 0,
                    PlayerIds = new[] { re2 },
                    SpecialCardIds = Array.Empty<int>(),
                    ExtraPoints = Array.Empty<object>(),
                },
                new
                {
                    Party = 1,
                    PlayerIds = new[] { kontra1 },
                    SpecialCardIds = Array.Empty<int>(),
                    ExtraPoints = Array.Empty<object>(),
                },
                new
                {
                    Party = 1,
                    PlayerIds = new[] { kontra2 },
                    SpecialCardIds = Array.Empty<int>(),
                    ExtraPoints = Array.Empty<object>(),
                },
            },
            Comment = comment,
        };

    [Fact]
    public async Task GetRounds_ReturnsOk()
    {
        var client = fixture.CreateClient();
        var response = await client.GetAsync("/analog/rounds");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateRound_ValidRequest_Returns201()
    {
        var client = fixture.CreateClient();
        var p1 = await CreatePlayerAsync(client);
        var p2 = await CreatePlayerAsync(client);
        var p3 = await CreatePlayerAsync(client);
        var p4 = await CreatePlayerAsync(client);

        var response = await client.PostAsJsonAsync(
            "/analog/rounds",
            BuildRoundBody(p1, p2, p3, p4)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetInt32().Should().BePositive();
    }

    [Fact]
    public async Task CreateRound_LessThan4Teams_Returns400()
    {
        var client = fixture.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/analog/rounds",
            new
            {
                PlayedAt = DateTime.UtcNow,
                WinningParty = 0,
                Points = 2,
                GameModeId = 3,
                Teams = new object[]
                {
                    new
                    {
                        Party = 0,
                        PlayerIds = new[] { 1 },
                        SpecialCardIds = Array.Empty<int>(),
                        ExtraPoints = Array.Empty<object>(),
                    },
                    new
                    {
                        Party = 1,
                        PlayerIds = new[] { 2 },
                        SpecialCardIds = Array.Empty<int>(),
                        ExtraPoints = Array.Empty<object>(),
                    },
                },
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRound_MissingParty_Returns400()
    {
        var client = fixture.CreateClient();
        var p1 = await CreatePlayerAsync(client);
        var p2 = await CreatePlayerAsync(client);
        var p3 = await CreatePlayerAsync(client);
        var p4 = await CreatePlayerAsync(client);

        var response = await client.PostAsJsonAsync(
            "/analog/rounds",
            new
            {
                PlayedAt = DateTime.UtcNow,
                WinningParty = 0,
                Points = 2,
                GameModeId = 3,
                Teams = new object[]
                {
                    new
                    {
                        Party = 0,
                        PlayerIds = new[] { p1 },
                        SpecialCardIds = Array.Empty<int>(),
                        ExtraPoints = Array.Empty<object>(),
                    },
                    new
                    {
                        Party = 0,
                        PlayerIds = new[] { p2 },
                        SpecialCardIds = Array.Empty<int>(),
                        ExtraPoints = Array.Empty<object>(),
                    },
                    new
                    {
                        Party = 0,
                        PlayerIds = new[] { p3 },
                        SpecialCardIds = Array.Empty<int>(),
                        ExtraPoints = Array.Empty<object>(),
                    },
                    new
                    {
                        Party = 0,
                        PlayerIds = new[] { p4 },
                        SpecialCardIds = Array.Empty<int>(),
                        ExtraPoints = Array.Empty<object>(),
                    },
                },
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("error").GetString().Should().Be("both_parties_required");
    }

    [Fact]
    public async Task GetRound_UnknownId_Returns404()
    {
        var client = fixture.CreateClient();
        var response = await client.GetAsync("/analog/rounds/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRound_ExistingId_ReturnsDetail()
    {
        var client = fixture.CreateClient();
        var p1 = await CreatePlayerAsync(client);
        var p2 = await CreatePlayerAsync(client);
        var p3 = await CreatePlayerAsync(client);
        var p4 = await CreatePlayerAsync(client);

        var createResponse = await client.PostAsJsonAsync(
            "/analog/rounds",
            BuildRoundBody(p1, p2, p3, p4, comment: "Testkommentar")
        );
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetInt32();

        var response = await client.GetAsync($"/analog/rounds/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetInt32().Should().Be(id);
        body.GetProperty("comment").GetString().Should().Be("Testkommentar");
        body.GetProperty("teams").GetArrayLength().Should().Be(4);
    }

    [Fact]
    public async Task GetRounds_NewestFirst()
    {
        var client = fixture.CreateClient();
        var p1 = await CreatePlayerAsync(client);
        var p2 = await CreatePlayerAsync(client);
        var p3 = await CreatePlayerAsync(client);
        var p4 = await CreatePlayerAsync(client);

        var id1Response = await client.PostAsJsonAsync(
            "/analog/rounds",
            BuildRoundBody(p1, p2, p3, p4)
        );
        var id2Response = await client.PostAsJsonAsync(
            "/analog/rounds",
            BuildRoundBody(p1, p2, p3, p4)
        );
        var id1 = (await id1Response.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id")
            .GetInt32();
        var id2 = (await id2Response.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id")
            .GetInt32();

        var response = await client.GetFromJsonAsync<JsonElement>(
            "/analog/rounds?page=1&pageSize=100"
        );
        var items = response.GetProperty("items").EnumerateArray().ToList();
        var ids = items.Select(i => i.GetProperty("id").GetInt32()).ToList();

        ids.IndexOf(id2).Should().BeLessThan(ids.IndexOf(id1));
    }

    [Fact]
    public async Task UpdateRound_ChangesData()
    {
        var client = fixture.CreateClient();
        var p1 = await CreatePlayerAsync(client);
        var p2 = await CreatePlayerAsync(client);
        var p3 = await CreatePlayerAsync(client);
        var p4 = await CreatePlayerAsync(client);

        var createResponse = await client.PostAsJsonAsync(
            "/analog/rounds",
            BuildRoundBody(p1, p2, p3, p4, comment: "Alt")
        );
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetInt32();

        var updateResponse = await client.PutAsJsonAsync(
            $"/analog/rounds/{id}",
            BuildRoundBody(p1, p2, p3, p4, comment: "Neu")
        );

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await client.GetFromJsonAsync<JsonElement>($"/analog/rounds/{id}");
        detail.GetProperty("comment").GetString().Should().Be("Neu");
    }

    [Fact]
    public async Task UpdateRound_UnknownId_Returns404()
    {
        var client = fixture.CreateClient();
        var p1 = await CreatePlayerAsync(client);
        var p2 = await CreatePlayerAsync(client);
        var p3 = await CreatePlayerAsync(client);
        var p4 = await CreatePlayerAsync(client);

        var response = await client.PutAsJsonAsync(
            "/analog/rounds/999999",
            BuildRoundBody(p1, p2, p3, p4)
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRound_Returns204_ThenGet404()
    {
        var client = fixture.CreateClient();
        var p1 = await CreatePlayerAsync(client);
        var p2 = await CreatePlayerAsync(client);
        var p3 = await CreatePlayerAsync(client);
        var p4 = await CreatePlayerAsync(client);

        var createResponse = await client.PostAsJsonAsync(
            "/analog/rounds",
            BuildRoundBody(p1, p2, p3, p4)
        );
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetInt32();

        var deleteResponse = await client.DeleteAsync($"/analog/rounds/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/analog/rounds/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRound_UnknownId_Returns404()
    {
        var client = fixture.CreateClient();
        var response = await client.DeleteAsync("/analog/rounds/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
