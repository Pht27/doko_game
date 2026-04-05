using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Application.Games.UseCases;

public interface IStartGameUseCase
{
    Task<GameActionResult<StartGameResult>> ExecuteAsync(StartGameCommand command, CancellationToken ct = default);
}

public sealed class StartGameUseCase(IGameRepository repository, IGameEventPublisher publisher) : IStartGameUseCase
{
    public async Task<GameActionResult<StartGameResult>> ExecuteAsync(StartGameCommand command, CancellationToken ct = default)
    {
        if (command.Players.Count != 4)
            return new GameActionResult<StartGameResult>.Failure(GameError.InvalidPhase);

        var players = command.Players
            .Select((id, i) => new PlayerState(id, (PlayerSeat)i, Domain.Hands.Hand.Empty, null))
            .ToList();

        var state = GameState.Create(
            rules:   command.Rules,
            players: players,
            phase:   GamePhase.Dealing);

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, [], ct);

        return new GameActionResult<StartGameResult>.Ok(new StartGameResult(state.Id));
    }
}
