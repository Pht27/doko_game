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
            execute: (ReservationState state) =>
            {
                if (state.Phase != GamePhase.ReservationHealthCheck)
                    return (
                        Fail<DeclareHealthStatusResult>(GameError.InvalidPhase),
                        [],
                        state
                    );

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

                GameState nextState = state;
                nextState = nextState.Apply(
                    new RecordHealthDeclarationModification(command.Player, command.HasVorbehalt)
                );

                (var remaining, nextState) = AdvancePendingQueue(nextState);
                if (remaining.Count > 0)
                    return (Ok(new DeclareHealthStatusResult(false)), events, nextState);

                nextState = ResolveNextPhaseAfterAllDeclared((ReservationState)nextState);
                return (Ok(new DeclareHealthStatusResult(true)), events, nextState);
            },
            ct
        );

    // ── Private helpers ───────────────────────────────────────────────────────

    private static (IReadOnlyList<PlayerSeat> remaining, GameState nextState) AdvancePendingQueue(
        GameState state
    )
    {
        var typed = (ReservationState)state;
        var remaining = typed.PendingReservationResponders.Skip(1).ToList();
        state = state.Apply(new SetPendingRespondersModification(remaining));
        if (remaining.Count > 0)
            state = state.Apply(new SetCurrentTurnModification(remaining[0]));
        return (remaining, state);
    }

    private static GameState ResolveNextPhaseAfterAllDeclared(ReservationState state)
    {
        var rauskommerSeat = (int)state.VorbehaltRauskommer;
        var vorbehaltPlayers = state
            .Players.Where(p => state.HealthDeclarations.TryGetValue(p.Seat, out var hasV) && hasV)
            .OrderBy(p => ((int)p.Seat - rauskommerSeat + 4) % 4)
            .Select(p => p.Seat)
            .ToList();

        GameState nextState = state;
        if (vorbehaltPlayers.Count == 0)
        {
            var silentMode = SilentModeDetector.Detect(state);
            // For Normalspiel set game-mode fields before transitioning; SilentMode must be set
            // AFTER transitioning because PlayingState is the first type that carries that field.
            if (silentMode is null)
                nextState = nextState.Apply(new SetGameModeModification(null, null));
            nextState = nextState.Apply(new AdvancePhaseModification(GamePhase.Playing));
            if (silentMode is not null)
                nextState = nextState.Apply(new SetSilentGameModeModification(silentMode));
            nextState = nextState.Apply(new SetCurrentTurnModification(state.VorbehaltRauskommer));
        }
        else
        {
            nextState = nextState.Apply(new SetPendingRespondersModification(vorbehaltPlayers));
            nextState = nextState.Apply(new AdvancePhaseModification(GamePhase.ReservationSoloCheck));
            nextState = nextState.Apply(new SetCurrentTurnModification(vorbehaltPlayers[0]));
        }

        return nextState;
    }
}
