using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Doko.Analog.IntegrationTests;

[Collection("Analog")]
public class AnalogStaticTests(AnalogTestFixture fixture)
{
    [Fact]
    public async Task GetStatic_ReturnsOk()
    {
        var client = fixture.CreateClient();
        var response = await client.GetAsync("/analog/static");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetStatic_Returns12GameModes()
    {
        var client = fixture.CreateClient();
        var body = await client.GetFromJsonAsync<JsonElement>("/analog/static");
        body.GetProperty("gameModes").GetArrayLength().Should().Be(12);
    }

    [Fact]
    public async Task GetStatic_Returns9SpecialCards()
    {
        var client = fixture.CreateClient();
        var body = await client.GetFromJsonAsync<JsonElement>("/analog/static");
        body.GetProperty("specialCards").GetArrayLength().Should().Be(9);
    }

    [Fact]
    public async Task GetStatic_Returns8ExtraPoints()
    {
        var client = fixture.CreateClient();
        var body = await client.GetFromJsonAsync<JsonElement>("/analog/static");
        body.GetProperty("extraPoints").GetArrayLength().Should().Be(8);
    }
}
