using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Handlers;
using Doko.Application.Games.Results;
using Doko.Application.Lobbies;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Rules;

namespace Doko.Api.Services;

public interface IOpaService
{
    /// <summary>
    /// Executes all pending Opa turns synchronously after a human player action.
    /// Returns GameFinishedResult if Opa played the last card of the game, otherwise null.
    /// </summary>
    Task<GameFinishedResult?> ExecuteOpaActionsAsync(GameId gameId, CancellationToken ct = default);
}

public sealed class OpaService(
    IGameRepository gameRepository,
    ILobbyRepository lobbyRepository,
    IDeclareHealthStatusHandler declareHealth,
    IAcceptArmutHandler acceptArmut,
    IChooseSchwarzesSauSoloHandler chooseSchwarzesSauSolo,
    IPlayCardHandler playCard
) : IOpaService
{
    public async Task<GameFinishedResult?> ExecuteOpaActionsAsync(
        GameId gameId,
        CancellationToken ct = default
    )
    {
        var lobby = await lobbyRepository.GetByGameIdAsync(gameId, ct);
        if (lobby is null || lobby.OpaSeats.Count == 0)
            return null;

        while (true)
        {
            var state = await gameRepository.GetAsync(gameId, ct);
            if (state is null)
                return null;

            if (!lobby.OpaSeats.Contains((int)state.CurrentTurn))
                return null;

            switch (state.Phase)
            {
                case GamePhase.ReservationHealthCheck:
                    await DeclareGesundAsync(gameId, state.CurrentTurn, ct);
                    break;

                case GamePhase.ArmutPartnerFinding:
                    await DeclineArmutAsync(gameId, state.CurrentTurn, ct);
                    break;

                case GamePhase.SchwarzesSauSoloSelect:
                    await ChooseFirstSchwarzesSauSoloAsync(gameId, state.CurrentTurn, ct);
                    break;

                case GamePhase.Playing:
                    var finished = await PlayFirstCardAsync(gameId, state, ct);
                    if (finished is not null)
                        return finished;
                    break;

                default:
                    return null;
            }
        }
    }

    private async Task DeclareGesundAsync(GameId gameId, PlayerSeat seat, CancellationToken ct)
    {
        await declareHealth.ExecuteAsync(
            new DeclareHealthStatusCommand(gameId, seat, HasVorbehalt: false),
            ct
        );
    }

    private async Task DeclineArmutAsync(GameId gameId, PlayerSeat seat, CancellationToken ct)
    {
        await acceptArmut.ExecuteAsync(new AcceptArmutCommand(gameId, seat, Accepts: false), ct);
    }

    private async Task ChooseFirstSchwarzesSauSoloAsync(
        GameId gameId,
        PlayerSeat seat,
        CancellationToken ct
    )
    {
        var command = new ChooseSchwarzesSauSoloCommand(gameId, seat, ReservationPriority.KaroSolo);
        await chooseSchwarzesSauSolo.ExecuteAsync(command, ct);
    }

    private async Task<GameFinishedResult?> PlayFirstCardAsync(
        GameId gameId,
        GameState state,
        CancellationToken ct
    )
    {
        var playerState = state.Players.First(p => p.Seat == state.CurrentTurn);

        var currentTrick = state.GetCurrentTrick();
        var card =
            currentTrick is null || currentTrick.Cards.Count == 0
                ? playerState.Hand.Cards[0]
                : playerState.Hand.Cards.First(c =>
                    CardPlayValidator.CanPlay(
                        c,
                        playerState.Hand,
                        currentTrick,
                        state.TrumpEvaluator
                    )
                );

        var command = new PlayCardCommand(gameId, state.CurrentTurn, card.Id, []);
        var result = await playCard.ExecuteAsync(command, ct);

        if (result is GameActionResult<PlayCardResult>.Ok ok && ok.Value.GameFinished)
            return ok.Value.FinishedResult;

        return null;
    }
}
