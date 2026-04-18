using Doko.Api.DTOs.Responses;
using Doko.Api.Hubs;
using Doko.Api.Services;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Handlers;
using Doko.Application.Lobbies;
using Doko.Application.Lobbies.Handlers;
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
    IJoinSeatHandler joinSeat,
    ILeaveLobbyHandler leaveLobby,
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
                token,
                SeatIndex: 0
            )
        );
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListLobbies(CancellationToken ct)
    {
        var lobbies = await lobbyRepository.GetAllAsync(ct);
        var response = lobbies
            .Select(l => new LobbyListItemResponse(
                l.Id.ToString(),
                l.Seats.Select(s => s != null).ToArray(),
                l.IsStarted
            ))
            .ToArray();
        return Ok(response);
    }

    [HttpPost("{lobbyId}/seats/{seatIndex:int}/join")]
    [AllowAnonymous]
    public async Task<IActionResult> JoinSeat(string lobbyId, int seatIndex, CancellationToken ct)
    {
        if (!Guid.TryParse(lobbyId, out var guid))
            return NotFound(new ErrorResponse("lobby_not_found"));

        var command = new JoinSeatCommand(new LobbyId(guid), seatIndex);
        var result = await joinSeat.ExecuteAsync(command, ct);

        if (result is LobbyActionResult<JoinSeatResult>.Failure failure)
            return failure.Error switch
            {
                LobbyError.LobbyNotFound => NotFound(new ErrorResponse("lobby_not_found")),
                LobbyError.LobbyAlreadyStarted => Conflict(
                    new ErrorResponse("lobby_already_started")
                ),
                LobbyError.SeatOccupied => Conflict(new ErrorResponse("seat_occupied")),
                _ => StatusCode(500, new ErrorResponse("unknown_error")),
            };

        var ok = ((LobbyActionResult<JoinSeatResult>.Ok)result).Value;

        await hub
            .Clients.Group($"lobby_{lobbyId}")
            .SendAsync("playerJoined", new { seatIndex, playerCount = ok.IsNowFull ? 4 : 0 }, ct);

        return Ok(
            new LobbyJoinResponse(
                lobbyId,
                ok.PlayerId.Value,
                tokenService.GenerateToken(ok.PlayerId),
                SeatIndex: seatIndex
            )
        );
    }

    [HttpPost("{lobbyId}/leave")]
    [Authorize]
    public async Task<IActionResult> LeaveLobby(string lobbyId, CancellationToken ct)
    {
        if (!Guid.TryParse(lobbyId, out var guid))
            return NotFound(new ErrorResponse("lobby_not_found"));

        var callerIdClaim = User.FindFirst("player_id")?.Value ?? "255";
        var callerId = new PlayerId((byte)int.Parse(callerIdClaim));

        var command = new LeaveLobbyCommand(new LobbyId(guid), callerId);
        var result = await leaveLobby.ExecuteAsync(command, ct);

        if (result is LobbyActionResult<LeaveLobbyResult>.Failure failure)
            return failure.Error switch
            {
                LobbyError.LobbyNotFound => NotFound(new ErrorResponse("lobby_not_found")),
                LobbyError.PlayerNotInLobby => Conflict(new ErrorResponse("player_not_in_lobby")),
                _ => StatusCode(500, new ErrorResponse("unknown_error")),
            };

        var ok = ((LobbyActionResult<LeaveLobbyResult>.Ok)result).Value;

        if (ok.LobbyDeleted)
            await hub.Clients.Group($"lobby_{lobbyId}").SendAsync("lobbyClosed", ct);
        else
            await hub
                .Clients.Group($"lobby_{lobbyId}")
                .SendAsync("playerLeft", new { seatIndex = callerId.Value }, ct);

        return NoContent();
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

        var seats = lobby.Seats.Select(s => s != null).ToArray();
        return Ok(
            new LobbyViewResponse(lobbyId, seats, lobby.IsStarted, lobby.Standings.ToArray())
        );
    }

    [HttpPost("{lobbyId}/new-game/ready")]
    [Authorize]
    public async Task<IActionResult> VoteNewGame(string lobbyId, CancellationToken ct)
    {
        if (!Guid.TryParse(lobbyId, out var guid))
            return NotFound(new ErrorResponse("lobby_not_found"));

        var lobby = await lobbyRepository.GetAsync(new LobbyId(guid), ct);
        if (lobby is null)
            return NotFound(new ErrorResponse("lobby_not_found"));

        var callerIdClaim = User.FindFirst("player_id")?.Value ?? "255";
        var callerId = new PlayerId((byte)int.Parse(callerIdClaim));
        if (!lobby.HasPlayer(callerId))
            return Forbid();

        bool allReady = lobby.AddNewGameVote(callerId);

        if (allReady)
        {
            var oldGameId = lobby.ActiveGameId?.ToString();
            lobby.MarkGameFinished();
            lobby.ResetNewGameVotes();
            lobby.AdvanceRauskommerIfRequired();

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

            var newGameId = startOk.Value.GameId;
            var vorbehaltRauskommer = new PlayerId((byte)lobby.VorbehaltRauskommer);
            await dealCards.ExecuteAsync(new DealCardsCommand(newGameId, vorbehaltRauskommer), ct);

            lobby.MarkStarted(newGameId);
            await lobbyRepository.SaveAsync(lobby, ct);

            if (oldGameId != null)
                await hub
                    .Clients.Group(oldGameId)
                    .SendAsync("newGameStarted", new { gameId = newGameId.ToString() }, ct);
        }
        else
        {
            await lobbyRepository.SaveAsync(lobby, ct);
            var gameGroupName = lobby.ActiveGameId?.ToString();
            if (gameGroupName != null)
                await hub
                    .Clients.Group(gameGroupName)
                    .SendAsync("newGameVoteChanged", new { count = lobby.NewGameVoteCount }, ct);
        }

        return Ok(new { voteCount = lobby.NewGameVoteCount });
    }

    [HttpPost("{lobbyId}/new-game/withdraw")]
    [Authorize]
    public async Task<IActionResult> WithdrawNewGame(string lobbyId, CancellationToken ct)
    {
        if (!Guid.TryParse(lobbyId, out var guid))
            return NotFound(new ErrorResponse("lobby_not_found"));

        var lobby = await lobbyRepository.GetAsync(new LobbyId(guid), ct);
        if (lobby is null)
            return NotFound(new ErrorResponse("lobby_not_found"));

        var callerIdClaim = User.FindFirst("player_id")?.Value ?? "255";
        var callerId = new PlayerId((byte)int.Parse(callerIdClaim));
        if (!lobby.HasPlayer(callerId))
            return Forbid();

        lobby.RemoveNewGameVote(callerId);
        await lobbyRepository.SaveAsync(lobby, ct);

        var gameGroupName = lobby.ActiveGameId?.ToString();
        if (gameGroupName != null)
            await hub
                .Clients.Group(gameGroupName)
                .SendAsync("newGameVoteChanged", new { count = lobby.NewGameVoteCount }, ct);

        return Ok(new { voteCount = lobby.NewGameVoteCount });
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

        // Allow restart: if the previous game is finished the lobby can host a new one
        if (lobby.IsStarted)
            lobby.MarkGameFinished();

        if (!lobby.IsFull)
            return BadRequest(new ErrorResponse("lobby_not_full"));

        var callerIdClaim = User.FindFirst("player_id")?.Value ?? "255";
        var callerId = new PlayerId((byte)int.Parse(callerIdClaim));
        if (!lobby.HasPlayer(callerId))
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

        lobby.MarkStarted(gameId);
        await lobbyRepository.SaveAsync(lobby, ct);

        var groupName = $"lobby_{lobbyId}";
        await hub
            .Clients.Group(groupName)
            .SendAsync("gameStarted", new { gameId = gameId.ToString() }, ct);

        return Ok(new StartLobbyGameResponse(gameId.ToString()));
    }
}
