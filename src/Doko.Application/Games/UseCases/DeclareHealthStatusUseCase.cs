using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.UseCases;

public interface IDeclareHealthStatusUseCase
{
    Task<GameActionResult<DeclareHealthStatusResult>> ExecuteAsync(
        DeclareHealthStatusCommand command,
        CancellationToken ct = default
    );
}

public sealed class DeclareHealthStatusUseCase(
    IGameRepository repository,
    IGameEventPublisher publisher
) : IDeclareHealthStatusUseCase
{
    public async Task<GameActionResult<DeclareHealthStatusResult>> ExecuteAsync(
        DeclareHealthStatusCommand command,
        CancellationToken ct = default
    )
    {
        var state = await repository.GetAsync(command.GameId, ct);
        if (state is null)
            return new GameActionResult<DeclareHealthStatusResult>.Failure(GameError.GameNotFound);

        if (state.Phase != GamePhase.ReservationHealthCheck)
            return new GameActionResult<DeclareHealthStatusResult>.Failure(GameError.InvalidPhase);

        // Must be the player's turn (first in pending queue)
        if (
            state.PendingReservationResponders.Count == 0
            || state.PendingReservationResponders[0] != command.Player
        )
            return new GameActionResult<DeclareHealthStatusResult>.Failure(GameError.NotYourTurn);

        if (state.HealthDeclarations.ContainsKey(command.Player))
            return new GameActionResult<DeclareHealthStatusResult>.Failure(GameError.AlreadyDeclared);

        var events = new List<IDomainEvent>
        {
            new HealthDeclaredEvent(state.Id, command.Player, command.HasVorbehalt),
        };

        state.Apply(new RecordHealthDeclarationModification(command.Player, command.HasVorbehalt));

        // Advance the pending queue
        var remaining = state.PendingReservationResponders.Skip(1).ToList();
        state.Apply(new SetPendingRespondersModification(remaining));

        if (remaining.Count > 0)
        {
            // More players still need to declare health
            state.Apply(new SetCurrentTurnModification(remaining[0]));
            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, events, ct);
            return new GameActionResult<DeclareHealthStatusResult>.Ok(
                new DeclareHealthStatusResult(false)
            );
        }

        // All players have declared — resolve next phase
        var vorbehaltPlayers = state
            .Players.Where(p =>
                state.HealthDeclarations.TryGetValue(p.Id, out var hasV) && hasV
            )
            .Select(p => p.Id)
            .ToList();

        if (vorbehaltPlayers.Count == 0)
        {
            // No reservation — normal game
            state.Apply(new SetGameModeModification(null));
            state.Apply(new AdvancePhaseModification(GamePhase.Playing));
            state.Apply(new SetCurrentTurnModification(state.Players[0].Id));
        }
        else
        {
            // One or more Vorbehalt players → move to Solo check
            state.Apply(new SetPendingRespondersModification(vorbehaltPlayers));
            state.Apply(new AdvancePhaseModification(GamePhase.ReservationSoloCheck));
            state.Apply(new SetCurrentTurnModification(vorbehaltPlayers[0]));
        }

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
        return new GameActionResult<DeclareHealthStatusResult>.Ok(
            new DeclareHealthStatusResult(true)
        );
    }
}
