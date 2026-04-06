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

/// <summary>
/// Handles reservation declarations in any reservation check phase
/// (SoloCheck, ArmutCheck, SchmeissenCheck, HochzeitCheck).
/// </summary>
public sealed class MakeReservationUseCase(
    IGameRepository repository,
    IGameEventPublisher publisher
) : IMakeReservationUseCase
{
    private static readonly IReadOnlySet<GamePhase> CheckPhases = new HashSet<GamePhase>
    {
        GamePhase.ReservationSoloCheck,
        GamePhase.ReservationArmutCheck,
        GamePhase.ReservationSchmeissenCheck,
        GamePhase.ReservationHochzeitCheck,
    };

    public async Task<GameActionResult<MakeReservationResult>> ExecuteAsync(
        MakeReservationCommand command,
        CancellationToken ct = default
    )
    {
        var state = await repository.GetAsync(command.GameId, ct);
        if (state is null)
            return new GameActionResult<MakeReservationResult>.Failure(GameError.GameNotFound);

        if (!CheckPhases.Contains(state.Phase))
            return new GameActionResult<MakeReservationResult>.Failure(GameError.InvalidPhase);

        // Must be the player's turn
        if (
            state.PendingReservationResponders.Count == 0
            || state.PendingReservationResponders[0] != command.Player
        )
            return new GameActionResult<MakeReservationResult>.Failure(GameError.NotYourTurn);

        if (state.ReservationDeclarations.ContainsKey(command.Player))
            return new GameActionResult<MakeReservationResult>.Failure(GameError.AlreadyDeclared);

        var playerState = state.Players.First(p => p.Id == command.Player);

        // Validate that the declared reservation is allowed in this phase
        if (!IsAllowedInPhase(command.Reservation, state.Phase, state))
            return new GameActionResult<MakeReservationResult>.Failure(
                GameError.ReservationNotEligible
            );

        // Validate eligibility
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

        var remaining = state.PendingReservationResponders.Skip(1).ToList();
        state.Apply(new SetPendingRespondersModification(remaining));

        if (remaining.Count > 0)
        {
            state.Apply(new SetCurrentTurnModification(remaining[0]));
            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, events, ct);
            return new GameActionResult<MakeReservationResult>.Ok(
                new MakeReservationResult(false, null)
            );
        }

        // All pending players have answered — resolve this phase
        var result = await ResolvePhaseAsync(state, events, ct);
        return result;
    }

    // ── Per-phase declaration validation ──────────────────────────────────────

    private static bool IsAllowedInPhase(
        IReservation? reservation,
        GamePhase phase,
        Domain.GameFlow.GameState state
    )
    {
        // Single-Vorbehalt player must declare something — passing is not allowed.
        bool singleVorbehalt = state.HealthDeclarations.Count(kv => kv.Value) == 1;
        if (reservation is null && phase == GamePhase.ReservationSoloCheck && singleVorbehalt)
            return false;

        // Passing is allowed in all other check phases
        if (reservation is null)
            return true;

        return phase switch
        {
            // Single-Vorbehalt player may declare any reservation in SoloCheck
            GamePhase.ReservationSoloCheck =>
                singleVorbehalt || IsSoloReservation(reservation),
            GamePhase.ReservationArmutCheck =>
                reservation is ArmutReservation,
            GamePhase.ReservationSchmeissenCheck =>
                reservation is SchmeissenReservation,
            GamePhase.ReservationHochzeitCheck =>
                reservation is HochzeitReservation,
            _ => false,
        };
    }

    private static bool IsSoloReservation(IReservation r) =>
        r is FarbsoloReservation
            or DamensoloReservation
            or BubensoloReservation
            or FleischlosesReservation
            or KnochenlosesReservation;

    // ── Phase resolution ──────────────────────────────────────────────────────

    private async Task<GameActionResult<MakeReservationResult>> ResolvePhaseAsync(
        Domain.GameFlow.GameState state,
        List<IDomainEvent> events,
        CancellationToken ct
    )
    {
        switch (state.Phase)
        {
            case GamePhase.ReservationSoloCheck:
                return await ResolveSoloCheckAsync(state, events, ct);

            case GamePhase.ReservationArmutCheck:
                return await ResolveArmutCheckAsync(state, events, ct);

            case GamePhase.ReservationSchmeissenCheck:
                return await ResolveSchmeissenCheckAsync(state, events, ct);

            case GamePhase.ReservationHochzeitCheck:
                return await ResolveHochzeitCheckAsync(state, events, ct);

            default:
                throw new InvalidOperationException($"Unexpected phase: {state.Phase}");
        }
    }

    private async Task<GameActionResult<MakeReservationResult>> ResolveSoloCheckAsync(
        Domain.GameFlow.GameState state,
        List<IDomainEvent> events,
        CancellationToken ct
    )
    {
        var winner = PickWinner(state);

        if (winner is not null)
        {
            // A Solo (or single-Vorbehalt free choice) won
            state.Apply(new SetGameModeModification(winner));
            state.Apply(new AdvancePhaseModification(GamePhase.Playing));
            state.Apply(new SetCurrentTurnModification(state.Players[0].Id));
            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, events, ct);
            return new GameActionResult<MakeReservationResult>.Ok(
                new MakeReservationResult(true, winner)
            );
        }

        // No Solo — advance to Armut check
        var vorbehaltPlayers = VorbehaltPlayers(state);
        AdvanceToNextCheckPhase(state, GamePhase.ReservationArmutCheck, vorbehaltPlayers);
        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
        return new GameActionResult<MakeReservationResult>.Ok(
            new MakeReservationResult(false, null)
        );
    }

    private async Task<GameActionResult<MakeReservationResult>> ResolveArmutCheckAsync(
        Domain.GameFlow.GameState state,
        List<IDomainEvent> events,
        CancellationToken ct
    )
    {
        var winner = PickWinner(state);

        if (winner is ArmutReservation armut)
        {
            // Find which player declared Armut (first in order = lowest seat)
            var armutPlayerId = FirstDeclarantId(state, winner);
            state.Apply(new SetArmutPlayerModification(armutPlayerId));

            // Determine partner-finding queue: players after poor player in seat order
            var partnerCandidates = PlayersAfter(state, armutPlayerId);
            state.Apply(new SetPendingRespondersModification(partnerCandidates));
            state.Apply(new AdvancePhaseModification(GamePhase.ArmutPartnerFinding));
            state.Apply(new SetCurrentTurnModification(partnerCandidates[0]));

            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, events, ct);
            return new GameActionResult<MakeReservationResult>.Ok(
                new MakeReservationResult(true, winner)
            );
        }

        // No Armut — advance to Schmeißen check
        var vorbehaltPlayers = VorbehaltPlayers(state);
        AdvanceToNextCheckPhase(state, GamePhase.ReservationSchmeissenCheck, vorbehaltPlayers);
        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
        return new GameActionResult<MakeReservationResult>.Ok(
            new MakeReservationResult(false, null)
        );
    }

    private async Task<GameActionResult<MakeReservationResult>> ResolveSchmeissenCheckAsync(
        Domain.GameFlow.GameState state,
        List<IDomainEvent> events,
        CancellationToken ct
    )
    {
        var winner = PickWinner(state);

        if (winner is SchmeissenReservation)
        {
            state.Apply(new AdvancePhaseModification(GamePhase.Geschmissen));
            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, events, ct);
            return new GameActionResult<MakeReservationResult>.Ok(
                new MakeReservationResult(true, winner, Geschmissen: true)
            );
        }

        // No Schmeißen — advance to Hochzeit check
        var vorbehaltPlayers = VorbehaltPlayers(state);
        AdvanceToNextCheckPhase(state, GamePhase.ReservationHochzeitCheck, vorbehaltPlayers);
        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
        return new GameActionResult<MakeReservationResult>.Ok(
            new MakeReservationResult(false, null)
        );
    }

    private async Task<GameActionResult<MakeReservationResult>> ResolveHochzeitCheckAsync(
        Domain.GameFlow.GameState state,
        List<IDomainEvent> events,
        CancellationToken ct
    )
    {
        var winner = PickWinner(state);

        if (winner is HochzeitReservation hochzeit)
        {
            state.Apply(new SetGameModeModification(hochzeit));
            state.Apply(new AdvancePhaseModification(GamePhase.Playing));
            state.Apply(new SetCurrentTurnModification(state.Players[0].Id));
            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, events, ct);
            return new GameActionResult<MakeReservationResult>.Ok(
                new MakeReservationResult(true, hochzeit)
            );
        }

        // No Hochzeit — force Schlanker Martin for the first Vorbehalt player
        var vorbehaltPlayers = VorbehaltPlayers(state);
        var martinPlayer = vorbehaltPlayers[0];
        var playerState = state.Players.First(p => p.Id == martinPlayer);
        var schlankerMartin = new SchlankerMartinReservation(martinPlayer);

        if (!schlankerMartin.IsEligible(playerState.Hand, state.Rules))
        {
            // Rules don't allow Schlanker Martin — fall through to normal game
            state.Apply(new SetGameModeModification(null));
        }
        else
        {
            state.Apply(new SetGameModeModification(schlankerMartin));
        }

        state.Apply(new AdvancePhaseModification(GamePhase.Playing));
        state.Apply(new SetCurrentTurnModification(state.Players[0].Id));
        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
        return new GameActionResult<MakeReservationResult>.Ok(
            new MakeReservationResult(true, state.ActiveReservation)
        );
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Picks the highest-priority winner from current declarations (lowest Priority value).
    /// Tie-breaks by the first player in seat order.
    /// </summary>
    private static IReservation? PickWinner(Domain.GameFlow.GameState state) =>
        state
            .ReservationDeclarations.Where(kv => kv.Value is not null)
            .OrderBy(kv => kv.Value!.Priority)
            .ThenBy(kv => (int)state.Players.First(p => p.Id == kv.Key).Seat)
            .Select(kv => kv.Value)
            .FirstOrDefault();

    /// <summary>Returns the player ID of the first declarant of the given reservation (by seat order).</summary>
    private static PlayerId FirstDeclarantId(
        Domain.GameFlow.GameState state,
        IReservation target
    ) =>
        state
            .ReservationDeclarations.Where(kv =>
                kv.Value?.Priority == target.Priority
            )
            .OrderBy(kv => (int)state.Players.First(p => p.Id == kv.Key).Seat)
            .First()
            .Key;

    /// <summary>Returns Vorbehalt players in seat order.</summary>
    private static IReadOnlyList<PlayerId> VorbehaltPlayers(
        Domain.GameFlow.GameState state
    ) =>
        state
            .Players.Where(p =>
                state.HealthDeclarations.TryGetValue(p.Id, out var hasV) && hasV
            )
            .Select(p => p.Id)
            .ToList();

    /// <summary>Returns players seated after <paramref name="player"/> (wrapping around), in seat order.</summary>
    private static IReadOnlyList<PlayerId> PlayersAfter(
        Domain.GameFlow.GameState state,
        PlayerId player
    )
    {
        int seat = (int)state.Players.First(p => p.Id == player).Seat;
        return state
            .Players.OrderBy(p => ((int)p.Seat - seat + 4) % 4)
            .Skip(1) // exclude the player themselves
            .Select(p => p.Id)
            .ToList();
    }

    private static void AdvanceToNextCheckPhase(
        Domain.GameFlow.GameState state,
        GamePhase next,
        IReadOnlyList<PlayerId> players
    )
    {
        state.Apply(new ClearReservationDeclarationsModification());
        state.Apply(new SetPendingRespondersModification(players));
        state.Apply(new AdvancePhaseModification(next));
        state.Apply(new SetCurrentTurnModification(players[0]));
    }
}
