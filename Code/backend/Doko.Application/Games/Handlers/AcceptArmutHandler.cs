using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.GameFlow.Modifications;
using Doko.Domain.Reservations;
using static Doko.Application.Common.GameActionResultExtensions;

namespace Doko.Application.Games.Handlers;

public interface IAcceptArmutHandler
{
    Task<GameActionResult<AcceptArmutResult>> ExecuteAsync(
        AcceptArmutCommand command,
        CancellationToken ct = default
    );
}

/// <summary>
/// Handles a player's acceptance or refusal of the Armut during
/// <see cref="GamePhase.ArmutPartnerFinding"/>.
/// </summary>
public sealed class AcceptArmutHandler(IGameRepository repository, IGameEventPublisher publisher)
    : IAcceptArmutHandler
{
    public Task<GameActionResult<AcceptArmutResult>> ExecuteAsync(
        AcceptArmutCommand command,
        CancellationToken ct = default
    ) =>
        GameCommandPipeline.RunAsync<AcceptArmutResult>(
            repository,
            publisher,
            command.GameId,
            GamePhase.ArmutPartnerFinding,
            execute: state =>
            {
                if (
                    state.PendingReservationResponders.Count == 0
                    || state.PendingReservationResponders[0] != command.Player
                )
                    return (Fail<AcceptArmutResult>(GameError.NotYourTurn), [], state);

                IReadOnlyList<IDomainEvent> events =
                [
                    new ArmutResponseEvent(state.Id, command.Player, command.Accepts),
                ];

                return command.Accepts
                    ? HandleAcceptance(state, command, events)
                    : HandleDecline(state, command, events);
            },
            ct
        );

    // ── Private helpers ───────────────────────────────────────────────────────

    private static (
        GameActionResult<AcceptArmutResult>,
        IReadOnlyList<IDomainEvent>,
        GameState
    ) HandleAcceptance(
        GameState state,
        AcceptArmutCommand command,
        IReadOnlyList<IDomainEvent> events
    )
    {
        state = state.Apply(new SetArmutRichPlayerModification(command.Player));
        state = state.Apply(new SetPendingRespondersModification([]));

        var poorPlayer = state.Armut!.Player;
        state = state.Apply(new ArmutGiveTrumpsModification(poorPlayer, command.Player));

        var reservation = new ArmutReservation(poorPlayer, command.Player);
        state = state.Apply(new SetGameModeModification(reservation, poorPlayer));

        state = state.Apply(new AdvancePhaseModification(GamePhase.ArmutCardExchange));
        state = state.Apply(new SetCurrentTurnModification(command.Player));

        return (Ok(new AcceptArmutResult(true)), events, state);
    }

    private static (
        GameActionResult<AcceptArmutResult>,
        IReadOnlyList<IDomainEvent>,
        GameState
    ) HandleDecline(GameState state, AcceptArmutCommand command, IReadOnlyList<IDomainEvent> events)
    {
        var remaining = state.PendingReservationResponders.Skip(1).ToList();
        state = state.Apply(new SetPendingRespondersModification(remaining));

        if (remaining.Count > 0)
        {
            state = state.Apply(new SetCurrentTurnModification(remaining[0]));
            return (Ok(new AcceptArmutResult(false)), events, state);
        }

        // Nobody accepted — Schwarze Sau
        state = state.Apply(new SetGameModeModification(null, null));
        state = state.Apply(new SetSchwarzesSauModification());
        state = state.Apply(new AdvancePhaseModification(GamePhase.Playing));
        state = state.Apply(new SetCurrentTurnModification(state.Armut!.Player));

        return (Ok(new AcceptArmutResult(false, SchwarzesSau: true)), events, state);
    }
}
