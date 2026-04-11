using Doko.Api.DTOs.Requests;
using Doko.Api.DTOs.Responses;
using Doko.Api.Extensions;
using Doko.Api.Hubs;
using Doko.Api.Mapping;
using Doko.Application.Abstractions;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Handlers;
using Doko.Application.Games.Queries;
using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
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
        return result.ToActionResult(r => Ok(DtoMapper.ToResponse(r)));
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
                await hub
                    .Clients.Group(gameId)
                    .SendAsync(
                        "gameFinished",
                        new { result = DtoMapper.ToDto(finished.Result) },
                        ct
                    );

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

        if (!Enum.TryParse<AnnouncementType>(req.Type, ignoreCase: true, out var announcementType))
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

    private PlayerId GetPlayerId()
    {
        var claim = User.FindFirst("player_id")?.Value ?? "0";
        return new PlayerId((byte)int.Parse(claim));
    }
}
