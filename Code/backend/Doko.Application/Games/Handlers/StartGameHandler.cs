using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Application.Games.Handlers;

public interface IStartGameHandler
{
    Task<GameActionResult<StartGameResult>> ExecuteAsync(
        StartGameCommand command,
        CancellationToken ct = default
    );
}

public sealed class StartGameHandler(IGameRepository repository, IGameEventPublisher publisher)
    : IStartGameHandler
{
    public async Task<GameActionResult<StartGameResult>> ExecuteAsync(
        StartGameCommand command,
        CancellationToken ct = default
    )
    {
        if (command.Players.Count != 4)
            return new GameActionResult<StartGameResult>.Failure(GameError.InvalidPhase);

        var players = command
            .Players.Select(seat => new PlayerState(seat, Domain.Hands.Hand.Empty, null))
            .ToList();

        var state = GameState.Create(
            rules: command.Rules,
            players: players,
            phase: GamePhase.Dealing
        );

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, [], ct);

        return new GameActionResult<StartGameResult>.Ok(new StartGameResult(state.Id));
    }
}
