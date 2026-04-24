using Doko.Api.DTOs.Requests;
using Doko.Api.DTOs.Responses;
using Doko.Api.Extensions;
using Doko.Api.Hubs;
using Doko.Api.Mapping;
using Doko.Api.Services;
using Doko.Application.Abstractions;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Handlers;
using Doko.Application.Games.Queries;
using Doko.Application.Lobbies;
using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Parties;
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
    IOpaService opaService,
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

        var players = req.PlayerIds.Select(id => (PlayerSeat)id).ToList();
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

        var player = GetPlayerSeat();
        var command = new DeclareHealthStatusCommand(new GameId(guid), player, req.HasVorbehalt);
        var result = await declareHealth.ExecuteAsync(command, ct);
        await opaService.ExecuteOpaActionsAsync(new GameId(guid), ct);
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

        var player = GetPlayerSeat();
        var reservation = DtoMapper.BuildReservation(req, player);
        var command = new MakeReservationCommand(new GameId(guid), player, reservation);
        var result = await makeReservation.ExecuteAsync(command, ct);
        await opaService.ExecuteOpaActionsAsync(new GameId(guid), ct);
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

        var player = GetPlayerSeat();
        var command = new AcceptArmutCommand(new GameId(guid), player, req.Accepts);
        var result = await acceptArmut.ExecuteAsync(command, ct);
        await opaService.ExecuteOpaActionsAsync(new GameId(guid), ct);
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

        var player = GetPlayerSeat();
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

        var player = GetPlayerSeat();
        var sonderkarten = req
            .ActivateSonderkarten.Where(s =>
                Enum.TryParse<SonderkarteType>(s, ignoreCase: true, out _)
            )
            .Select(s => Enum.Parse<SonderkarteType>(s, ignoreCase: true))
            .ToList();
        var genscherPartner = req.GenscherPartnerId.HasValue
            ? (PlayerSeat?)req.GenscherPartnerId.Value
            : null;

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
            if (r.GameFinished && r.FinishedResult is { } humanFinished)
            {
                await HandleGameFinishedAsync(gameId, new GameId(guid), humanFinished, ct);
            }
            else
            {
                var opaFinished = await opaService.ExecuteOpaActionsAsync(new GameId(guid), ct);
                if (opaFinished is not null)
                    await HandleGameFinishedAsync(gameId, new GameId(guid), opaFinished, ct);
            }

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

        var player = GetPlayerSeat();
        var command = new MakeAnnouncementCommand(new GameId(guid), player, announcementType);
        var result = await makeAnnouncement.ExecuteAsync(command, ct);
        return result.ToActionResult(_ => Ok());
    }

    [HttpGet("{gameId}")]
    public async Task<IActionResult> GetGame(string gameId, CancellationToken ct)
    {
        if (!Guid.TryParse(gameId, out var guid))
            return NotFound(new ErrorResponse("game_not_found"));

        var player = GetPlayerSeat();
        var view = await gameQuery.GetPlayerViewAsync(new GameId(guid), player, ct);
        if (view is null)
            return NotFound(new ErrorResponse("game_not_found"));

        var response = DtoMapper.ToResponse(view);

        if (view.Phase == GamePhase.Finished)
        {
            var lobby = await lobbyRepository.GetByGameIdAsync(new GameId(guid), ct);
            if (lobby?.GameHistory.Count > 0)
            {
                var (lastResult, gameMode, netPoints, partyPerSeat) = lobby.GameHistory[^1];
                var standings = lobby.Standings.ToArray();
                var matchHistory = BuildMatchHistory(lobby.GameHistory.SkipLast(1).ToList());
                response = response with
                {
                    FinishedResult = DtoMapper.ToDto(
                        lastResult,
                        netPoints,
                        standings,
                        partyPerSeat: partyPerSeat,
                        matchHistory: matchHistory,
                        gameMode: gameMode
                    ),
                    NewGameVoteCount = lobby.NewGameVoteCount,
                };
            }
        }

        return Ok(response);
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
            .SendAsync(
                "gameFinished",
                new { result = DtoMapper.ToGeschmissenDto(standings, matchHistory) },
                ct
            );
    }

    private async Task HandleGameFinishedAsync(
        string gameIdString,
        GameId gameId,
        Application.Games.Results.GameFinishedResult finished,
        CancellationToken ct
    )
    {
        var netPoints = finished.NetPointsPerSeat.ToArray();
        var partyPerSeat = finished.PartyPerSeat.ToArray();
        int[]? standings = null;
        IReadOnlyList<GameResultDto>? matchHistory = null;

        var lobby = await lobbyRepository.GetByGameIdAsync(gameId, ct);
        if (lobby != null)
        {
            lobby.UpdateStandings(netPoints);
            lobby.AddGameRecord(finished.Result, finished.GameMode, netPoints, partyPerSeat);
            lobby.SetAdvanceRauskommer(finished.ShouldAdvanceRauskommer);
            standings = lobby.Standings.ToArray();
            matchHistory = BuildMatchHistory(lobby.GameHistory.SkipLast(1).ToList());
            await lobbyRepository.SaveAsync(lobby, ct);
        }

        await hub
            .Clients.Group(gameIdString)
            .SendAsync(
                "gameFinished",
                new
                {
                    result = DtoMapper.ToDto(
                        finished.Result,
                        netPoints,
                        standings,
                        partyPerSeat: partyPerSeat,
                        matchHistory: matchHistory,
                        gameMode: finished.GameMode
                    ),
                },
                ct
            );
    }

    private static IReadOnlyList<GameResultDto> BuildMatchHistory(
        IEnumerable<(
            GameResult Result,
            string? GameMode,
            int[] NetPoints,
            Party?[] PartyPerSeat
        )> history
    ) =>
        history
            .Select(e =>
                DtoMapper.ToDto(
                    e.Result,
                    e.NetPoints,
                    partyPerSeat: e.PartyPerSeat,
                    gameMode: e.GameMode
                )
            )
            .ToList();

    private PlayerSeat GetPlayerSeat()
    {
        var claim = User.FindFirst("seat_index")?.Value ?? "0";
        return (PlayerSeat)int.Parse(claim);
    }
}
