using Doko.Api.DTOs.Responses;
using Doko.Api.Hubs;
using Doko.Api.Services;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Handlers;
using Doko.Application.Lobbies;
using Doko.Application.Lobbies.Handlers;
using Doko.Application.Scenarios;
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
    ISwapSeatHandler swapSeat,
    IAddOpaHandler addOpa,
    IRemoveOpaHandler removeOpa,
    ILobbyRepository lobbyRepository,
    IStartGameHandler startGame,
    IDealCardsHandler dealCards,
    ITokenService tokenService,
    IOpaService opaService,
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

        var token = tokenService.GenerateToken(ok.Value.Seat);
        return Ok(new LobbyJoinResponse(ok.Value.LobbyId.ToString(), token, SeatIndex: 0));
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
                l.IsStarted,
                l.CreatedAt
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
                LobbyError.SeatOccupied => Conflict(new ErrorResponse("seat_occupied")),
                _ => StatusCode(500, new ErrorResponse("unknown_error")),
            };

        var ok = ((LobbyActionResult<JoinSeatResult>.Ok)result).Value;

        await hub
            .Clients.Group($"lobby_{lobbyId}")
            .SendAsync("playerJoined", new { seatIndex, playerCount = ok.IsNowFull ? 4 : 0 }, ct);

        // If a game is already running, include its id in the response so the client
        // can navigate straight to it without a second round-trip.
        var lobby = await lobbyRepository.GetAsync(new LobbyId(guid), ct);
        string? activeGameId = (lobby?.IsStarted == true) ? lobby.ActiveGameId?.ToString() : null;

        return Ok(
            new LobbyJoinResponse(
                lobbyId,
                tokenService.GenerateToken(ok.Seat),
                SeatIndex: seatIndex,
                ActiveGameId: activeGameId
            )
        );
    }

    [HttpPost("{lobbyId}/seats/{seatIndex:int}/swap")]
    [Authorize]
    public async Task<IActionResult> SwapSeat(string lobbyId, int seatIndex, CancellationToken ct)
    {
        if (!Guid.TryParse(lobbyId, out var guid))
            return NotFound(new ErrorResponse("lobby_not_found"));

        var callerSeat = GetCallerSeat();
        var command = new SwapSeatCommand(new LobbyId(guid), callerSeat, seatIndex);
        var result = await swapSeat.ExecuteAsync(command, ct);

        if (result is LobbyActionResult<SwapSeatResult>.Failure failure)
            return failure.Error switch
            {
                LobbyError.LobbyNotFound => NotFound(new ErrorResponse("lobby_not_found")),
                LobbyError.PlayerNotInLobby => Conflict(new ErrorResponse("player_not_in_lobby")),
                LobbyError.SeatOccupied => Conflict(new ErrorResponse("seat_occupied")),
                _ => StatusCode(500, new ErrorResponse("unknown_error")),
            };

        var ok = ((LobbyActionResult<SwapSeatResult>.Ok)result).Value;
        var oldSeatIndex = (int)callerSeat;

        await hub
            .Clients.Group($"lobby_{lobbyId}")
            .SendAsync("playerLeft", new { seatIndex = oldSeatIndex }, ct);
        await hub
            .Clients.Group($"lobby_{lobbyId}")
            .SendAsync("playerJoined", new { seatIndex, playerCount = 0 }, ct);

        return Ok(
            new LobbyJoinResponse(
                lobbyId,
                tokenService.GenerateToken(ok.NewSeat),
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

        var callerId = GetCallerSeat();

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
        {
            await hub.Clients.Group($"lobby_{lobbyId}").SendAsync("lobbyClosed", ct);
        }
        else
        {
            await hub
                .Clients.Group($"lobby_{lobbyId}")
                .SendAsync("playerLeft", new { seatIndex = (int)callerId }, ct);

            if (ok.ActiveGameId != null)
                await hub
                    .Clients.Group(ok.ActiveGameId)
                    .SendAsync("newGameVoteChanged", new { count = ok.NewGameVoteCount }, ct);
        }

        return NoContent();
    }

    [HttpPost("{lobbyId}/seats/{seatIndex:int}/opa")]
    [Authorize]
    public async Task<IActionResult> AddOpa(string lobbyId, int seatIndex, CancellationToken ct)
    {
        if (!Guid.TryParse(lobbyId, out var guid))
            return NotFound(new ErrorResponse("lobby_not_found"));

        var lobby = await lobbyRepository.GetAsync(new LobbyId(guid), ct);
        if (lobby is null)
            return NotFound(new ErrorResponse("lobby_not_found"));

        var callerId = GetCallerSeat();
        if (!lobby.HasPlayer(callerId))
            return Forbid();

        if (lobby.IsStarted)
            return Conflict(new ErrorResponse("lobby_already_started"));

        var command = new AddOpaCommand(new LobbyId(guid), seatIndex);
        var result = await addOpa.ExecuteAsync(command, ct);

        if (result is LobbyActionResult<AddOpaResult>.Failure failure)
            return failure.Error switch
            {
                LobbyError.LobbyNotFound => NotFound(new ErrorResponse("lobby_not_found")),
                LobbyError.SeatOccupied => Conflict(new ErrorResponse("lobby_full")),
                _ => StatusCode(500, new ErrorResponse("unknown_error")),
            };

        var updatedLobby = await lobbyRepository.GetAsync(new LobbyId(guid), ct);

        await hub
            .Clients.Group($"lobby_{lobbyId}")
            .SendAsync(
                "playerJoined",
                new
                {
                    seatIndex,
                    playerCount = 0,
                    isOpa = true,
                },
                ct
            );

        await hub
            .Clients.Group($"lobby_{lobbyId}")
            .SendAsync(
                "lobbyReadyVoteChanged",
                new { count = updatedLobby?.LobbyStartVoteCount ?? 0 },
                ct
            );

        return Ok(new { seatIndex });
    }

    [HttpDelete("{lobbyId}/seats/{seatIndex:int}/opa")]
    [Authorize]
    public async Task<IActionResult> RemoveOpa(string lobbyId, int seatIndex, CancellationToken ct)
    {
        if (!Guid.TryParse(lobbyId, out var guid))
            return NotFound(new ErrorResponse("lobby_not_found"));

        var lobby = await lobbyRepository.GetAsync(new LobbyId(guid), ct);
        if (lobby is null)
            return NotFound(new ErrorResponse("lobby_not_found"));

        var callerId = GetCallerSeat();
        if (!lobby.HasPlayer(callerId))
            return Forbid();

        if (lobby.IsStarted)
            return Conflict(new ErrorResponse("lobby_already_started"));

        var command = new RemoveOpaCommand(new LobbyId(guid), seatIndex);
        var result = await removeOpa.ExecuteAsync(command, ct);

        if (result is LobbyActionResult<RemoveOpaResult>.Failure failure)
            return failure.Error switch
            {
                LobbyError.LobbyNotFound => NotFound(new ErrorResponse("lobby_not_found")),
                LobbyError.PlayerNotInLobby => NotFound(new ErrorResponse("opa_not_found")),
                _ => StatusCode(500, new ErrorResponse("unknown_error")),
            };

        var updatedLobby = await lobbyRepository.GetAsync(new LobbyId(guid), ct);

        await hub.Clients.Group($"lobby_{lobbyId}").SendAsync("playerLeft", new { seatIndex }, ct);

        await hub
            .Clients.Group($"lobby_{lobbyId}")
            .SendAsync(
                "lobbyReadyVoteChanged",
                new { count = updatedLobby?.LobbyStartVoteCount ?? 0 },
                ct
            );

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
            new LobbyViewResponse(
                lobbyId,
                seats,
                lobby.IsStarted,
                lobby.Standings.ToArray(),
                lobby.LobbyStartVoteCount,
                lobby.ActiveGameId?.ToString(),
                lobby.OpaSeats.ToArray(),
                lobby.SelectedScenario
            )
        );
    }

    [HttpPost("{lobbyId}/ready")]
    [Authorize]
    public async Task<IActionResult> VoteLobbyReady(string lobbyId, CancellationToken ct)
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

        var callerId = GetCallerSeat();
        if (!lobby.HasPlayer(callerId))
            return Forbid();

        bool allReady = lobby.AddLobbyStartVote(callerId);

        if (allReady)
        {
            lobby.ResetLobbyStartVotes();

            var players = lobby.Players.Select(p => p.Seat).ToList();
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
            await dealCards.ExecuteAsync(
                new DealCardsCommand(gameId, ScenarioName: lobby.SelectedScenario),
                ct
            );

            lobby.MarkStarted(gameId);
            await lobbyRepository.SaveAsync(lobby, ct);

            await hub
                .Clients.Group($"lobby_{lobbyId}")
                .SendAsync("gameStarted", new { gameId = gameId.ToString() }, ct);

            // Trigger Opa if it needs to act first (e.g. Opa is VorbehaltRauskommer)
            await opaService.ExecuteOpaActionsAsync(gameId, ct);
        }
        else
        {
            await lobbyRepository.SaveAsync(lobby, ct);
            await hub
                .Clients.Group($"lobby_{lobbyId}")
                .SendAsync("lobbyReadyVoteChanged", new { count = lobby.LobbyStartVoteCount }, ct);
        }

        return Ok(new { voteCount = lobby.LobbyStartVoteCount });
    }

    [HttpPost("{lobbyId}/ready/withdraw")]
    [Authorize]
    public async Task<IActionResult> WithdrawLobbyReady(string lobbyId, CancellationToken ct)
    {
        if (!Guid.TryParse(lobbyId, out var guid))
            return NotFound(new ErrorResponse("lobby_not_found"));

        var lobby = await lobbyRepository.GetAsync(new LobbyId(guid), ct);
        if (lobby is null)
            return NotFound(new ErrorResponse("lobby_not_found"));

        var callerId = GetCallerSeat();
        if (!lobby.HasPlayer(callerId))
            return Forbid();

        lobby.RemoveLobbyStartVote(callerId);
        await lobbyRepository.SaveAsync(lobby, ct);

        await hub
            .Clients.Group($"lobby_{lobbyId}")
            .SendAsync("lobbyReadyVoteChanged", new { count = lobby.LobbyStartVoteCount }, ct);

        return Ok(new { voteCount = lobby.LobbyStartVoteCount });
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

        var callerId = GetCallerSeat();
        if (!lobby.HasPlayer(callerId))
            return Forbid();

        bool allReady = lobby.AddNewGameVote(callerId);

        // Opa automatically votes for every new game
        foreach (var opaSeatIndex in lobby.OpaSeats)
            allReady = lobby.AddNewGameVote((PlayerSeat)opaSeatIndex) || allReady;

        if (allReady)
        {
            var oldGameId = lobby.ActiveGameId?.ToString();
            lobby.MarkGameFinished();
            lobby.ResetNewGameVotes();
            lobby.AdvanceRauskommerIfRequired();

            var players = lobby.Players.Select(p => p.Seat).ToList();
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
            var vorbehaltRauskommer = (PlayerSeat)lobby.VorbehaltRauskommer;
            await dealCards.ExecuteAsync(
                new DealCardsCommand(newGameId, vorbehaltRauskommer, lobby.SelectedScenario),
                ct
            );

            lobby.MarkStarted(newGameId);
            await lobbyRepository.SaveAsync(lobby, ct);

            if (oldGameId != null)
                await hub
                    .Clients.Group(oldGameId)
                    .SendAsync("newGameStarted", new { gameId = newGameId.ToString() }, ct);

            // Trigger Opa if it needs to act first in the new game
            await opaService.ExecuteOpaActionsAsync(newGameId, ct);
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

        var callerId = GetCallerSeat();
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

        var callerId = GetCallerSeat();
        if (!lobby.HasPlayer(callerId))
            return Forbid();

        var players = lobby.Players.Select(p => p.Seat).ToList();
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

    [HttpGet("scenarios")]
    [AllowAnonymous]
    public IActionResult GetScenarios() =>
        Ok(new ScenarioListResponse(Scenarios.All.Select(s => s.Name).ToArray()));

    [HttpPost("{lobbyId}/scenario")]
    [Authorize]
    public async Task<IActionResult> SetScenario(
        string lobbyId,
        [FromBody] SetScenarioRequest body,
        CancellationToken ct
    )
    {
        if (!Guid.TryParse(lobbyId, out var guid))
            return NotFound(new ErrorResponse("lobby_not_found"));

        var lobby = await lobbyRepository.GetAsync(new LobbyId(guid), ct);
        if (lobby is null)
            return NotFound(new ErrorResponse("lobby_not_found"));

        var callerId = GetCallerSeat();
        if (!lobby.HasPlayer(callerId))
            return Forbid();

        if (body.Name is not null && !Scenarios.All.Any(s => s.Name == body.Name))
            return BadRequest(new ErrorResponse("scenario_not_found"));

        lobby.SetScenario(body.Name);
        await lobbyRepository.SaveAsync(lobby, ct);

        await hub
            .Clients.Group($"lobby_{lobbyId}")
            .SendAsync("scenarioChanged", new { name = body.Name }, ct);

        return Ok(new { name = body.Name });
    }

    private PlayerSeat GetCallerSeat()
    {
        var claim = User.FindFirst("seat_index")?.Value ?? "0";
        return (PlayerSeat)int.Parse(claim);
    }
}

public record SetScenarioRequest(string? Name);
