using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Doko.Api.DTOs.Responses;
using Doko.Api.IntegrationTests.Stubs;
using Doko.Application.Common;
using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;

namespace Doko.Api.IntegrationTests;

public class GamesControllerTests(ApiTestFixture fixture) : IClassFixture<ApiTestFixture>
{
    [Fact]
    public async Task StartGame_Unauthenticated_Returns401()
    {
        var client = fixture.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/games",
            new { PlayerIds = new[] { 0, 1, 2, 3 } }
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task StartGame_ValidRequest_Returns200WithGameId()
    {
        var expectedId = GameId.New();
        ((StubStartGameUseCase)fixture.StartGame).Handler =
            _ => new GameActionResult<StartGameResult>.Ok(new StartGameResult(expectedId));

        var client = fixture.CreateAuthenticatedClient(playerId: 0);
        var response = await client.PostAsJsonAsync(
            "/games",
            new { PlayerIds = new[] { 0, 1, 2, 3 } }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("gameId").GetString().Should().Be(expectedId.ToString());
    }

    [Fact]
    public async Task StartGame_WrongPlayerCount_Returns400()
    {
        var client = fixture.CreateAuthenticatedClient();
        var response = await client.PostAsJsonAsync(
            "/games",
            new { PlayerIds = new[] { 0, 1, 2 } }
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StartGame_UseCaseReturnsGameNotFound_Returns404()
    {
        ((StubStartGameUseCase)fixture.StartGame).Handler =
            _ => new GameActionResult<StartGameResult>.Failure(GameError.GameNotFound);

        var client = fixture.CreateAuthenticatedClient();
        var response = await client.PostAsJsonAsync(
            "/games",
            new { PlayerIds = new[] { 0, 1, 2, 3 } }
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DealCards_Unauthenticated_Returns401()
    {
        var gameId = Guid.NewGuid();
        var client = fixture.CreateClient();
        var response = await client.PostAsync($"/games/{gameId}/deal", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DealCards_ValidRequest_Returns200()
    {
        var gameId = Guid.NewGuid();
        ((StubDealCardsUseCase)fixture.DealCards).Handler = _ => new GameActionResult<Unit>.Ok(
            Unit.Value
        );

        var client = fixture.CreateAuthenticatedClient();
        var response = await client.PostAsync($"/games/{gameId}/deal", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DealCards_InvalidPhase_Returns400WithCode()
    {
        var gameId = Guid.NewGuid();
        ((StubDealCardsUseCase)fixture.DealCards).Handler = _ => new GameActionResult<Unit>.Failure(
            GameError.InvalidPhase
        );

        var client = fixture.CreateAuthenticatedClient();
        var response = await client.PostAsync($"/games/{gameId}/deal", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Error.Should().Be("invalid_phase");
    }

    [Fact]
    public async Task GetGame_Unauthenticated_Returns401()
    {
        var gameId = Guid.NewGuid();
        var client = fixture.CreateClient();
        var response = await client.GetAsync($"/games/{gameId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGame_InvalidGameId_Returns404()
    {
        var client = fixture.CreateAuthenticatedClient();
        var response = await client.GetAsync("/games/not-a-guid");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PlayCard_NotYourTurn_Returns400WithCode()
    {
        var gameId = Guid.NewGuid();
        ((StubPlayCardUseCase)fixture.PlayCard).Handler =
            _ => new GameActionResult<PlayCardResult>.Failure(GameError.NotYourTurn);

        var client = fixture.CreateAuthenticatedClient();
        var response = await client.PostAsJsonAsync(
            $"/games/{gameId}/cards",
            new
            {
                CardId = 5,
                ActivateSonderkarten = Array.Empty<string>(),
                GenscherPartnerId = (int?)null,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Error.Should().Be("not_your_turn");
    }

    [Fact]
    public async Task MakeAnnouncement_InvalidType_Returns400()
    {
        var gameId = Guid.NewGuid();
        var client = fixture.CreateAuthenticatedClient();
        var response = await client.PostAsJsonAsync(
            $"/games/{gameId}/announcements",
            new { Type = "NotAValidType" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MakeAnnouncement_ValidType_Returns200()
    {
        var gameId = Guid.NewGuid();
        ((StubMakeAnnouncementUseCase)fixture.MakeAnnouncement).Handler =
            _ => new GameActionResult<Unit>.Ok(Unit.Value);

        var client = fixture.CreateAuthenticatedClient();
        var response = await client.PostAsJsonAsync(
            $"/games/{gameId}/announcements",
            new { Type = "Re" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
