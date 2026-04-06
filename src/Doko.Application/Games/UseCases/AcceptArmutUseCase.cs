using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Reservations;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.UseCases;

public interface IAcceptArmutUseCase
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
public sealed class AcceptArmutUseCase(IGameRepository repository, IGameEventPublisher publisher)
    : IAcceptArmutUseCase
{
    public async Task<GameActionResult<AcceptArmutResult>> ExecuteAsync(
        AcceptArmutCommand command,
        CancellationToken ct = default
    )
    {
        var state = await repository.GetAsync(command.GameId, ct);
        if (state is null)
            return new GameActionResult<AcceptArmutResult>.Failure(GameError.GameNotFound);

        if (state.Phase != GamePhase.ArmutPartnerFinding)
            return new GameActionResult<AcceptArmutResult>.Failure(GameError.InvalidPhase);

        if (
            state.PendingReservationResponders.Count == 0
            || state.PendingReservationResponders[0] != command.Player
        )
            return new GameActionResult<AcceptArmutResult>.Failure(GameError.NotYourTurn);

        var events = new List<IDomainEvent>
        {
            new ArmutResponseEvent(state.Id, command.Player, command.Accepts),
        };

        if (command.Accepts)
        {
            // Partner found — enter card exchange phase
            state.Apply(new SetArmutRichPlayerModification(command.Player));
            state.Apply(new SetPendingRespondersModification([]));

            // Automatically transfer poor player's trumps to rich player
            var poorPlayer = state.ArmutPlayer!.Value;
            state.Apply(new ArmutGiveTrumpsModification(poorPlayer, command.Player));

            // Set game mode now that both players are known
            var reservation = new ArmutReservation(poorPlayer, command.Player);
            state.Apply(new SetGameModeModification(reservation));

            state.Apply(new AdvancePhaseModification(GamePhase.ArmutCardExchange));
            // Rich player exchanges the cards
            state.Apply(new SetCurrentTurnModification(command.Player));

            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, events, ct);
            return new GameActionResult<AcceptArmutResult>.Ok(new AcceptArmutResult(true));
        }

        // Declined — move to next candidate
        var remaining = state.PendingReservationResponders.Skip(1).ToList();
        state.Apply(new SetPendingRespondersModification(remaining));

        if (remaining.Count > 0)
        {
            state.Apply(new SetCurrentTurnModification(remaining[0]));
            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, events, ct);
            return new GameActionResult<AcceptArmutResult>.Ok(new AcceptArmutResult(false));
        }

        // Nobody accepted — Schwarze Sau
        // Poor player starts; normal game mode (no partner resolution yet)
        state.Apply(new SetGameModeModification(null));
        state.Apply(new AdvancePhaseModification(GamePhase.Playing));
        state.Apply(new SetCurrentTurnModification(state.ArmutPlayer!.Value));

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
        return new GameActionResult<AcceptArmutResult>.Ok(
            new AcceptArmutResult(false, SchwarzesSau: true)
        );
    }
}
