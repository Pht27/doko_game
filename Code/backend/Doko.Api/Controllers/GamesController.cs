using Doko.Api.DTOs.Requests;
using Doko.Api.DTOs.Responses;
using Doko.Api.Extensions;
using Doko.Api.Hubs;
using Doko.Api.Mapping;
using Doko.Application.Abstractions;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Handlers;
using Doko.Application.Games.Queries;
using Doko.Application.Lobbies;
using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

#pragma warning disable IDE0060 // unused parameter warnings for ct in simple wrappers

namespace Doko.Api.Controllers;

[ApiController]
[Route("games")]
[Authorize]
public class GamesController(
    IStartGameHandler startGame,
    IDealCardsHandler dealCards,
    IDeclareHealthStatusHandler declareHealth,
    IMakeReservationHandler makeReservation,
    IAcceptArmutHandler acceptArmut,
    IExchangeArmutCardsHandler exchangeArmutCards,
    IPlayCardHandler playCard,
    IMakeAnnouncementHandler makeAnnouncement,
    IGameQueryService gameQuery,
    ILobbyRepository lobbyRepository,
    IHubContext<GameHub> hub
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> StartGame(
        [FromBody] StartGameRequest req,
        CancellationToken ct
    )
    {
        if (req.PlayerIds.Count != 4)
            return BadRequest(new ErrorResponse("exactly_four_players_required"));

        var players = req.PlayerIds.Select(id => new PlayerId((byte)id)).ToList();
        var command = new StartGameCommand(players, Rules: null);
        var result = await startGame.ExecuteAsync(command, ct);
        return result.ToActionResult(r => Ok(new StartGameResponse(r.GameId.ToString())));
    }

    [HttpPost("{gameId}/deal")]
    public async Task<IActionResult> DealCards(string gameId, CancellationToken ct)
    {
        if (!Guid.TryParse(gameId, out var guid))
            return NotFound(new ErrorResponse("game_not_found"));

        var command = new DealCardsCommand(new GameId(guid));
        var result = await dealCards.ExecuteAsync(command, ct);
        return result.ToActionResult(_ => Ok());
    }

    [HttpPost("{gameId}/health")]
    public async Task<IActionResult> DeclareHealth(
        string gameId,
        [FromBody] DeclareHealthRequest req,
        CancellationToken ct
    )
    {
        if (!Guid.TryParse(gameId, out var guid))
            return NotFound(new ErrorResponse("game_not_found"));

        var player = GetPlayerId();
        var command = new DeclareHealthStatusCommand(new GameId(guid), player, req.HasVorbehalt);
        var result = await declareHealth.ExecuteAsync(command, ct);
        return result.ToActionResult(r => Ok(new DeclareHealthResponse(r.AllDeclared)));
    }

    [HttpPost("{gameId}/reservations")]
    public async Task<IActionResult> MakeReservation(
        string gameId,
        [FromBody] MakeReservationRequest req,
        CancellationToken ct
    )
    {
        if (!Guid.TryParse(gameId, out var guid))
            return NotFound(new ErrorResponse("game_not_found"));

        var player = GetPlayerId();
        var reservation = DtoMapper.BuildReservation(req, player);
        var command = new MakeReservationCommand(new GameId(guid), player, reservation);
        var result = await makeReservation.ExecuteAsync(command, ct);
        return await result.ToActionResult(async r =>
        {
            if (r.Geschmissen)
                await HandleGeschmissenAsync(gameId, new GameId(guid), ct);
            return Ok(DtoMapper.ToResponse(r));
        });
    }

    [HttpPost("{gameId}/armut-response")]
    public async Task<IActionResult> AcceptArmut(
        string gameId,
        [FromBody] AcceptArmutRequest req,
        CancellationToken ct
    )
    {
        if (!Guid.TryParse(gameId, out var guid))
            return NotFound(new ErrorResponse("game_not_found"));

        var player = GetPlayerId();
        var command = new AcceptArmutCommand(new GameId(guid), player, req.Accepts);
        var result = await acceptArmut.ExecuteAsync(command, ct);
        return result.ToActionResult(r => Ok(new AcceptArmutResponse(r.Accepted, r.SchwarzesSau)));
    }

    [HttpPost("{gameId}/armut-exchange")]
    public async Task<IActionResult> ExchangeArmutCards(
        string gameId,
        [FromBody] ExchangeArmutCardsRequest req,
        CancellationToken ct
    )
    {
        if (!Guid.TryParse(gameId, out var guid))
            return NotFound(new ErrorResponse("game_not_found"));

        var player = GetPlayerId();
        var cardIds = req.CardIds.Select(id => new CardId((byte)id)).ToList();
        var command = new ExchangeArmutCardsCommand(new GameId(guid), player, cardIds);
        var result = await exchangeArmutCards.ExecuteAsync(command, ct);
        return result.ToActionResult(r => Ok(new ExchangeArmutCardsResponse(r.ReturnedTrumpCount)));
    }

    [HttpPost("{gameId}/cards")]
    public async Task<IActionResult> PlayCard(
        string gameId,
        [FromBody] PlayCardRequest req,
        CancellationToken ct
    )
    {
        if (!Guid.TryParse(gameId, out var guid))
            return NotFound(new ErrorResponse("game_not_found"));

        var player = GetPlayerId();
        var sonderkarten = req
            .ActivateSonderkarten.Where(s =>
                Enum.TryParse<SonderkarteType>(s, ignoreCase: true, out _)
            )
            .Select(s => Enum.Parse<SonderkarteType>(s, ignoreCase: true))
            .ToList();
        var genscherPartner = req.GenscherPartnerId.HasValue
            ? new PlayerId((byte)req.GenscherPartnerId.Value)
            : (PlayerId?)null;

        var command = new PlayCardCommand(
            new GameId(guid),
            player,
            new CardId((byte)req.CardId),
            sonderkarten,
            genscherPartner
        );

        var result = await playCard.ExecuteAsync(command, ct);
        return await result.ToActionResult(async r =>
        {
            if (r.GameFinished && r.FinishedResult is { } finished)
                await HandleGameFinishedAsync(gameId, new GameId(guid), finished, ct);

            return Ok(DtoMapper.ToResponse(r));
        });
    }

    [HttpPost("{gameId}/announcements")]
    public async Task<IActionResult> MakeAnnouncement(
        string gameId,
        [FromBody] MakeAnnouncementRequest req,
        CancellationToken ct
    )
    {
        if (!Guid.TryParse(gameId, out var guid))
            return NotFound(new ErrorResponse("game_not_found"));

        // "Re" and "Kontra" are legacy aliases for "Win" (party is determined from the player)
        var typeString = req.Type is "Re" or "Kontra" ? "Win" : req.Type;
        if (
            !Enum.TryParse<AnnouncementType>(typeString, ignoreCase: true, out var announcementType)
        )
            return BadRequest(new ErrorResponse("invalid_announcement_type"));

        var player = GetPlayerId();
        var command = new MakeAnnouncementCommand(new GameId(guid), player, announcementType);
        var result = await makeAnnouncement.ExecuteAsync(command, ct);
        return result.ToActionResult(_ => Ok());
    }

    [HttpGet("{gameId}")]
    public async Task<IActionResult> GetGame(string gameId, CancellationToken ct)
    {
        if (!Guid.TryParse(gameId, out var guid))
            return NotFound(new ErrorResponse("game_not_found"));

        var player = GetPlayerId();
        var view = await gameQuery.GetPlayerViewAsync(new GameId(guid), player, ct);
        if (view is null)
            return NotFound(new ErrorResponse("game_not_found"));

        return Ok(DtoMapper.ToResponse(view));
    }

    private async Task HandleGeschmissenAsync(
        string gameIdString,
        GameId gameId,
        CancellationToken ct
    )
    {
        var lobby = await lobbyRepository.GetByGameIdAsync(gameId, ct);
        int[]? standings = null;
        IReadOnlyList<GameResultDto>? matchHistory = null;
        if (lobby != null)
        {
            lobby.SetAdvanceRauskommer(false);
            standings = lobby.Standings.ToArray();
            matchHistory = BuildMatchHistory(lobby.GameHistory);
            await lobbyRepository.SaveAsync(lobby, ct);
        }

        await hub
            .Clients.Group(gameIdString)
            .SendAsync("gameFinished", new { result = DtoMapper.ToGeschmissenDto(standings, matchHistory) }, ct);
    }

    private async Task HandleGameFinishedAsync(
        string gameIdString,
        GameId gameId,
        Application.Games.Results.GameFinishedResult finished,
        CancellationToken ct
    )
    {
        var netPoints = finished.NetPointsPerSeat.ToArray();
        int[]? standings = null;
        IReadOnlyList<GameResultDto>? matchHistory = null;

        var lobby = await lobbyRepository.GetByGameIdAsync(gameId, ct);
        if (lobby != null)
        {
            lobby.UpdateStandings(netPoints);
            lobby.AddGameRecord(finished.Result, netPoints);
            lobby.SetAdvanceRauskommer(finished.ShouldAdvanceRauskommer);
            standings = lobby.Standings.ToArray();
            matchHistory = BuildMatchHistory(lobby.GameHistory.SkipLast(1).ToList());
            await lobbyRepository.SaveAsync(lobby, ct);
        }

        await hub
            .Clients.Group(gameIdString)
            .SendAsync(
                "gameFinished",
                new { result = DtoMapper.ToDto(finished.Result, netPoints, standings, matchHistory: matchHistory) },
                ct
            );
    }

    private static IReadOnlyList<GameResultDto> BuildMatchHistory(
        IEnumerable<(GameResult Result, int[] NetPoints)> history
    ) =>
        history.Select(e => DtoMapper.ToDto(e.Result, e.NetPoints)).ToList();

    private PlayerId GetPlayerId()
    {
        var claim = User.FindFirst("player_id")?.Value ?? "0";
        return new PlayerId((byte)int.Parse(claim));
    }
}
