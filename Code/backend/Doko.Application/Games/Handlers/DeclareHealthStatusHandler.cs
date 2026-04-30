using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.GameFlow.Modifications;
using Doko.Domain.Players;
using static Doko.Application.Common.GameActionResultExtensions;

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
    public Task<GameActionResult<DeclareHealthStatusResult>> ExecuteAsync(
        DeclareHealthStatusCommand command,
        CancellationToken ct = default
    ) =>
        GameCommandPipeline.RunAsync<DeclareHealthStatusResult, ReservationState>(
            repository,
            publisher,
            command.GameId,
            execute: (ReservationState typedState) =>
            {
                if (typedState.Phase != GamePhase.ReservationHealthCheck)
                    return (
                        Fail<DeclareHealthStatusResult>(GameError.InvalidPhase),
                        [],
                        typedState
                    );

                GameState state = typedState;
                if (
                    state.PendingReservationResponders.Count == 0
                    || state.PendingReservationResponders[0] != command.Player
                )
                    return (Fail<DeclareHealthStatusResult>(GameError.NotYourTurn), [], state);

                if (state.HealthDeclarations.ContainsKey(command.Player))
                    return (Fail<DeclareHealthStatusResult>(GameError.AlreadyDeclared), [], state);

                IReadOnlyList<IDomainEvent> events =
                [
                    new HealthDeclaredEvent(state.Id, command.Player, command.HasVorbehalt),
                ];

                state = state.Apply(
                    new RecordHealthDeclarationModification(command.Player, command.HasVorbehalt)
                );

                (var remaining, state) = AdvancePendingQueue(state);
                if (remaining.Count > 0)
                    return (Ok(new DeclareHealthStatusResult(false)), events, state);

                state = ResolveNextPhaseAfterAllDeclared(state);
                return (Ok(new DeclareHealthStatusResult(true)), events, state);
            },
            ct
        );

    // ── Private helpers ───────────────────────────────────────────────────────

    private static (IReadOnlyList<PlayerSeat> remaining, GameState nextState) AdvancePendingQueue(
        GameState state
    )
    {
        var remaining = state.PendingReservationResponders.Skip(1).ToList();
        state = state.Apply(new SetPendingRespondersModification(remaining));
        if (remaining.Count > 0)
            state = state.Apply(new SetCurrentTurnModification(remaining[0]));
        return (remaining, state);
    }

    private static GameState ResolveNextPhaseAfterAllDeclared(GameState state)
    {
        var rauskommerSeat = (int)state.VorbehaltRauskommer;
        var vorbehaltPlayers = state
            .Players.Where(p => state.HealthDeclarations.TryGetValue(p.Seat, out var hasV) && hasV)
            .OrderBy(p => ((int)p.Seat - rauskommerSeat + 4) % 4)
            .Select(p => p.Seat)
            .ToList();

        if (vorbehaltPlayers.Count == 0)
        {
            var silentMode = SilentModeDetector.Detect(state);
            if (silentMode is not null)
                state = state.Apply(new SetSilentGameModeModification(silentMode));
            else
                state = state.Apply(new SetGameModeModification(null, null));
            state = state.Apply(new AdvancePhaseModification(GamePhase.Playing));
            state = state.Apply(new SetCurrentTurnModification(state.VorbehaltRauskommer));
        }
        else
        {
            state = state.Apply(new SetPendingRespondersModification(vorbehaltPlayers));
            state = state.Apply(new AdvancePhaseModification(GamePhase.ReservationSoloCheck));
            state = state.Apply(new SetCurrentTurnModification(vorbehaltPlayers[0]));
        }

        return state;
    }
}
