using Doko.Api.DTOs.Responses;
using Doko.Api.Hubs;
using Doko.Api.Services;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Handlers;
using Doko.Application.Lobbies;
using Doko.Application.Lobbies.Handlers;
using Doko.Application.Lobbies.Queries;
using Doko.Domain.GameFlow;
using Doko.Domain.Lobby;
using Doko.Domain.Players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Doko.Api.Controllers;

[ApiController]
[Route("lobbies")]
public class LobbiesController(
    ICreateLobbyHandler createLobby,
    IJoinLobbyHandler joinLobby,
    ILobbyRepository lobbyRepository,
    IStartGameHandler startGame,
    IDealCardsHandler dealCards,
    ITokenService tokenService,
    IHubContext<GameHub> hub
) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateLobby(CancellationToken ct)
    {
        var result = await createLobby.ExecuteAsync(ct);
        if (result is not LobbyActionResult<CreateLobbyResult>.Ok ok)
            return StatusCode(500, new ErrorResponse("lobby_creation_failed"));

        var token = tokenService.GenerateToken(ok.Value.PlayerId);
        return Ok(
            new LobbyJoinResponse(
                ok.Value.LobbyId.ToString(),
                ok.Value.PlayerId.Value,
                IsHost: true,
                token,
                PlayerCount: 1
            )
        );
    }

    [HttpPost("{lobbyId}/join")]
    [AllowAnonymous]
    public async Task<IActionResult> JoinLobby(string lobbyId, CancellationToken ct)
    {
        if (!Guid.TryParse(lobbyId, out var guid))
            return NotFound(new ErrorResponse("lobby_not_found"));

        var result = await joinLobby.ExecuteAsync(new LobbyId(guid), ct);

        if (result is LobbyActionResult<JoinLobbyResult>.Failure failure)
            return failure.Error switch
            {
                LobbyError.LobbyNotFound => NotFound(new ErrorResponse("lobby_not_found")),
                LobbyError.LobbyFull => Conflict(new ErrorResponse("lobby_full")),
                LobbyError.LobbyAlreadyStarted => Conflict(
                    new ErrorResponse("lobby_already_started")
                ),
                _ => StatusCode(500, new ErrorResponse("unknown_error")),
            };

        var ok = ((LobbyActionResult<JoinLobbyResult>.Ok)result).Value;
        var playerCount = ok.PlayerId.Value + 1;

        await hub
            .Clients.Group($"lobby_{lobbyId}")
            .SendAsync("playerJoined", new { playerCount }, ct);

        return Ok(
            new LobbyJoinResponse(
                lobbyId,
                ok.PlayerId.Value,
                IsHost: false,
                tokenService.GenerateToken(ok.PlayerId),
                PlayerCount: playerCount
            )
        );
    }

    [HttpGet("{lobbyId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLobby(string lobbyId, CancellationToken ct)
    {
        if (!Guid.TryParse(lobbyId, out var guid))
            return NotFound(new ErrorResponse("lobby_not_found"));

        var lobby = await lobbyRepository.GetAsync(new LobbyId(guid), ct);
        if (lobby is null)
            return NotFound(new ErrorResponse("lobby_not_found"));

        var view = new LobbyView(lobby.Id, lobby.Players.Count, lobby.IsFull, lobby.IsStarted);
        return Ok(
            new LobbyViewResponse(
                view.LobbyId.ToString(),
                view.PlayerCount,
                view.IsFull,
                view.IsStarted
            )
        );
    }

    [HttpPost("{lobbyId}/start")]
    [Authorize]
    public async Task<IActionResult> StartLobbyGame(string lobbyId, CancellationToken ct)
    {
        if (!Guid.TryParse(lobbyId, out var guid))
            return NotFound(new ErrorResponse("lobby_not_found"));

        var lobby = await lobbyRepository.GetAsync(new LobbyId(guid), ct);
        if (lobby is null)
            return NotFound(new ErrorResponse("lobby_not_found"));

        if (lobby.IsStarted)
            return Conflict(new ErrorResponse("lobby_already_started"));

        if (!lobby.IsFull)
            return BadRequest(new ErrorResponse("lobby_not_full"));

        var callerIdClaim = User.FindFirst("player_id")?.Value ?? "255";
        var callerId = new PlayerId((byte)int.Parse(callerIdClaim));
        if (callerId != lobby.HostId)
            return Forbid();

        var players = lobby.Players.Select(p => p.Id).ToList();
        var startResult = await startGame.ExecuteAsync(
            new StartGameCommand(players, Rules: null),
            ct
        );
        if (
            startResult
            is not Application.Common.GameActionResult<Application.Games.Results.StartGameResult>.Ok startOk
        )
            return StatusCode(500, new ErrorResponse("game_start_failed"));

        var gameId = startOk.Value.GameId;
        await dealCards.ExecuteAsync(new DealCardsCommand(gameId), ct);

        lobby.MarkStarted();
        await lobbyRepository.SaveAsync(lobby, ct);

        var groupName = $"lobby_{lobbyId}";
        await hub
            .Clients.Group(groupName)
            .SendAsync("gameStarted", new { gameId = gameId.ToString() }, ct);

        return Ok(new StartLobbyGameResponse(gameId.ToString()));
    }
}
