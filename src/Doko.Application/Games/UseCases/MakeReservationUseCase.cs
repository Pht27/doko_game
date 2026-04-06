using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.UseCases;

public interface IMakeReservationUseCase
{
    Task<GameActionResult<MakeReservationResult>> ExecuteAsync(
        MakeReservationCommand command,
        CancellationToken ct = default
    );
}

public sealed class MakeReservationUseCase(
    IGameRepository repository,
    IGameEventPublisher publisher
) : IMakeReservationUseCase
{
    public async Task<GameActionResult<MakeReservationResult>> ExecuteAsync(
        MakeReservationCommand command,
        CancellationToken ct = default
    )
    {
        var state = await repository.GetAsync(command.GameId, ct);
        if (state is null)
            return new GameActionResult<MakeReservationResult>.Failure(GameError.GameNotFound);

        if (state.Phase != GamePhase.Reservations)
            return new GameActionResult<MakeReservationResult>.Failure(GameError.InvalidPhase);

        if (state.ReservationDeclarations.ContainsKey(command.Player))
            return new GameActionResult<MakeReservationResult>.Failure(GameError.AlreadyDeclared);

        var playerState = state.Players.FirstOrDefault(p => p.Id == command.Player);
        if (playerState is null)
            return new GameActionResult<MakeReservationResult>.Failure(GameError.NotYourTurn);

        if (
            command.Reservation is not null
            && !command.Reservation.IsEligible(playerState.Hand, state.Rules)
        )
            return new GameActionResult<MakeReservationResult>.Failure(
                GameError.ReservationNotEligible
            );

        var events = new List<IDomainEvent>
        {
            new ReservationMadeEvent(state.Id, command.Player, command.Reservation),
        };

        state.Apply(new RecordDeclarationModification(command.Player, command.Reservation));

        bool allDeclared = state.Players.All(p => state.ReservationDeclarations.ContainsKey(p.Id));
        if (!allDeclared)
        {
            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, events, ct);
            return new GameActionResult<MakeReservationResult>.Ok(
                new MakeReservationResult(false, null)
            );
        }

        // All declared — resolve winner by priority (lowest Priority value wins)
        var winner = state
            .ReservationDeclarations.Where(kv => kv.Value is not null)
            .OrderBy(kv => kv.Value!.Priority)
            .Select(kv => kv.Value)
            .FirstOrDefault();

        // Schmeißen ends the game immediately — no play, no score
        if (winner is SchmeissenReservation)
        {
            state.Apply(new AdvancePhaseModification(GamePhase.Geschmissen));
            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, events, ct);
            return new GameActionResult<MakeReservationResult>.Ok(
                new MakeReservationResult(true, winner, Geschmissen: true)
            );
        }

        state.Apply(new SetGameModeModification(winner));
        state.Apply(new AdvancePhaseModification(GamePhase.Playing));

        // First player leads
        state.Apply(new SetCurrentTurnModification(state.Players[0].Id));

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);

        return new GameActionResult<MakeReservationResult>.Ok(
            new MakeReservationResult(true, winner)
        );
    }
}
