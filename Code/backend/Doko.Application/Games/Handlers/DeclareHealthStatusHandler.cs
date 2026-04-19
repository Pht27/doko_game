using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Players;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.Handlers;

public interface IDeclareHealthStatusHandler
{
    Task<GameActionResult<DeclareHealthStatusResult>> ExecuteAsync(
        DeclareHealthStatusCommand command,
        CancellationToken ct = default
    );
}

public sealed class DeclareHealthStatusHandler(
    IGameRepository repository,
    IGameEventPublisher publisher
) : IDeclareHealthStatusHandler
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
            return new GameActionResult<DeclareHealthStatusResult>.Failure(
                GameError.AlreadyDeclared
            );

        var events = new List<IDomainEvent>
        {
            new HealthDeclaredEvent(state.Id, command.Player, command.HasVorbehalt),
        };

        state.Apply(new RecordHealthDeclarationModification(command.Player, command.HasVorbehalt));

        var remaining = AdvancePendingQueue(state);

        if (remaining.Count > 0)
            return await SaveAndReturn(state, events, new DeclareHealthStatusResult(false), ct);

        ResolveNextPhaseAfterAllDeclared(state);
        return await SaveAndReturn(state, events, new DeclareHealthStatusResult(true), ct);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static IReadOnlyList<PlayerSeat> AdvancePendingQueue(GameState state)
    {
        var remaining = state.PendingReservationResponders.Skip(1).ToList();
        state.Apply(new SetPendingRespondersModification(remaining));
        if (remaining.Count > 0)
            state.Apply(new SetCurrentTurnModification(remaining[0]));
        return remaining;
    }

    private static void ResolveNextPhaseAfterAllDeclared(GameState state)
    {
        var rauskommerSeat = (int)state.VorbehaltRauskommer;
        var vorbehaltPlayers = state
            .Players.Where(p => state.HealthDeclarations.TryGetValue(p.Seat, out var hasV) && hasV)
            .OrderBy(p => ((int)p.Seat - rauskommerSeat + 4) % 4)
            .Select(p => p.Seat)
            .ToList();

        if (vorbehaltPlayers.Count == 0)
        {
            // No reservation — normal game; VorbehaltRauskommer plays the first card
            state.Apply(new SetGameModeModification(null));
            state.Apply(new AdvancePhaseModification(GamePhase.Playing));
            state.Apply(new SetCurrentTurnModification(state.VorbehaltRauskommer));
        }
        else
        {
            // One or more Vorbehalt players → move to Solo check, ordered from VorbehaltRauskommer
            state.Apply(new SetPendingRespondersModification(vorbehaltPlayers));
            state.Apply(new AdvancePhaseModification(GamePhase.ReservationSoloCheck));
            state.Apply(new SetCurrentTurnModification(vorbehaltPlayers[0]));
        }
    }

    private async Task<GameActionResult<DeclareHealthStatusResult>> SaveAndReturn(
        GameState state,
        List<IDomainEvent> events,
        DeclareHealthStatusResult result,
        CancellationToken ct
    )
    {
        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
        return new GameActionResult<DeclareHealthStatusResult>.Ok(result);
    }
}
