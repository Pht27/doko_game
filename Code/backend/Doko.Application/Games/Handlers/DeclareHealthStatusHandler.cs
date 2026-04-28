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
        GameCommandPipeline.RunAsync<DeclareHealthStatusResult>(
            repository,
            publisher,
            command.GameId,
            GamePhase.ReservationHealthCheck,
            execute: state =>
            {
                if (
                    state.PendingReservationResponders.Count == 0
                    || state.PendingReservationResponders[0] != command.Player
                )
                    return (Fail<DeclareHealthStatusResult>(GameError.NotYourTurn), []);

                if (state.HealthDeclarations.ContainsKey(command.Player))
                    return (Fail<DeclareHealthStatusResult>(GameError.AlreadyDeclared), []);

                IReadOnlyList<IDomainEvent> events =
                [
                    new HealthDeclaredEvent(state.Id, command.Player, command.HasVorbehalt),
                ];

                state.Apply(
                    new RecordHealthDeclarationModification(command.Player, command.HasVorbehalt)
                );

                var remaining = AdvancePendingQueue(state);
                if (remaining.Count > 0)
                    return (Ok(new DeclareHealthStatusResult(false)), events);

                ResolveNextPhaseAfterAllDeclared(state);
                return (Ok(new DeclareHealthStatusResult(true)), events);
            },
            ct
        );

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
            var silentMode = SilentModeDetector.Detect(state);
            if (silentMode is not null)
                state.Apply(new SetSilentGameModeModification(silentMode));
            else
                state.Apply(new SetGameModeModification(null, null));
            state.Apply(new AdvancePhaseModification(GamePhase.Playing));
            state.Apply(new SetCurrentTurnModification(state.VorbehaltRauskommer));
        }
        else
        {
            state.Apply(new SetPendingRespondersModification(vorbehaltPlayers));
            state.Apply(new AdvancePhaseModification(GamePhase.ReservationSoloCheck));
            state.Apply(new SetCurrentTurnModification(vorbehaltPlayers[0]));
        }
    }
}
