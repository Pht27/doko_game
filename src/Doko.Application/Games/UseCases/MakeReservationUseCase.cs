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

        var validationError = Validate(command, state);
        if (validationError is not null)
            return new GameActionResult<MakeReservationResult>.Failure(validationError.Value);

        var events = new List<IDomainEvent>
        {
            new ReservationMadeEvent(state.Id, command.Player, command.Reservation),
        };

        RecordAndAdvanceQueue(command, state);

        if (state.PendingReservationResponders.Count > 0)
            return await SaveAndReturnOk(state, events, new MakeReservationResult(false, null), ct);

        return await ResolvePhaseAsync(state, events, ct);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    /// <summary>Returns the first validation error for the command, or null if valid.</summary>
    private static GameError? Validate(
        MakeReservationCommand command,
        Domain.GameFlow.GameState state
    )
    {
        if (!CheckPhases.Contains(state.Phase))
            return GameError.InvalidPhase;

        if (
            state.PendingReservationResponders.Count == 0
            || state.PendingReservationResponders[0] != command.Player
        )
            return GameError.NotYourTurn;

        if (state.ReservationDeclarations.ContainsKey(command.Player))
            return GameError.AlreadyDeclared;

        if (!IsAllowedInPhase(command.Reservation, state.Phase, state))
            return GameError.ReservationNotEligible;

        var playerState = state.Players.First(p => p.Id == command.Player);
        if (
            command.Reservation is not null
            && !command.Reservation.IsEligible(playerState.Hand, state.Rules)
        )
            return GameError.ReservationNotEligible;

        return null;
    }

    /// <summary>Records the declaration and removes the player from the pending queue.</summary>
    private static void RecordAndAdvanceQueue(
        MakeReservationCommand command,
        Domain.GameFlow.GameState state
    )
    {
        state.Apply(new RecordDeclarationModification(command.Player, command.Reservation));
        var remaining = state.PendingReservationResponders.Skip(1).ToList();
        state.Apply(new SetPendingRespondersModification(remaining));
        if (remaining.Count > 0)
            state.Apply(new SetCurrentTurnModification(remaining[0]));
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
            GamePhase.ReservationSoloCheck => singleVorbehalt || IsSoloReservation(reservation),
            GamePhase.ReservationArmutCheck => reservation is ArmutReservation,
            GamePhase.ReservationSchmeissenCheck => reservation is SchmeissenReservation,
            GamePhase.ReservationHochzeitCheck => reservation is HochzeitReservation,
            _ => false,
        };
    }

    private static bool IsSoloReservation(IReservation r) =>
        r
            is FarbsoloReservation
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

        if (winner is ArmutReservation)
            return await ResolveArmutCheckAsync(state, events, ct);

        if (winner is SchmeissenReservation)
            return await ResolveSchmeissenCheckAsync(state, events, ct);

        if (winner is not null)
        {
            // A Solo (or single-Vorbehalt free choice: Hochzeit, Schlanker Martin) won
            ApplyWinnerAndStartPlaying(state, winner);
            return await SaveAndReturnOk(
                state,
                events,
                new MakeReservationResult(true, winner),
                ct
            );
        }

        // No Solo — advance to Armut check
        AdvanceToNextCheckPhase(state, GamePhase.ReservationArmutCheck, VorbehaltPlayers(state));
        return await SaveAndReturnOk(state, events, new MakeReservationResult(false, null), ct);
    }

    private async Task<GameActionResult<MakeReservationResult>> ResolveArmutCheckAsync(
        Domain.GameFlow.GameState state,
        List<IDomainEvent> events,
        CancellationToken ct
    )
    {
        var winner = PickWinner(state);

        if (winner is ArmutReservation)
        {
            // Find which player declared Armut (first in order = lowest seat)
            var armutPlayerId = FirstDeclarantId(state, winner);
            state.Apply(new SetArmutPlayerModification(armutPlayerId));

            // Determine partner-finding queue: players after poor player in seat order
            var partnerCandidates = PlayersAfter(state, armutPlayerId);
            state.Apply(new SetPendingRespondersModification(partnerCandidates));
            state.Apply(new AdvancePhaseModification(GamePhase.ArmutPartnerFinding));
            state.Apply(new SetCurrentTurnModification(partnerCandidates[0]));

            return await SaveAndReturnOk(
                state,
                events,
                new MakeReservationResult(true, winner),
                ct
            );
        }

        // No Armut — advance to Schmeißen check
        AdvanceToNextCheckPhase(
            state,
            GamePhase.ReservationSchmeissenCheck,
            VorbehaltPlayers(state)
        );
        return await SaveAndReturnOk(state, events, new MakeReservationResult(false, null), ct);
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
            return await SaveAndReturnOk(
                state,
                events,
                new MakeReservationResult(true, winner, Geschmissen: true),
                ct
            );
        }

        // No Schmeißen — advance to Hochzeit check
        AdvanceToNextCheckPhase(state, GamePhase.ReservationHochzeitCheck, VorbehaltPlayers(state));
        return await SaveAndReturnOk(state, events, new MakeReservationResult(false, null), ct);
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
            ApplyWinnerAndStartPlaying(state, hochzeit);
            return await SaveAndReturnOk(
                state,
                events,
                new MakeReservationResult(true, hochzeit),
                ct
            );
        }

        // No Hochzeit — force Schlanker Martin for the first Vorbehalt player, or normal game
        ApplyFallbackGameMode(state, VorbehaltPlayers(state)[0]);
        return await SaveAndReturnOk(
            state,
            events,
            new MakeReservationResult(true, state.ActiveReservation),
            ct
        );
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Saves state, publishes events, and returns Ok with the given result.</summary>
    private async Task<GameActionResult<MakeReservationResult>> SaveAndReturnOk(
        Domain.GameFlow.GameState state,
        List<IDomainEvent> events,
        MakeReservationResult result,
        CancellationToken ct
    )
    {
        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
        return new GameActionResult<MakeReservationResult>.Ok(result);
    }

    /// <summary>Sets the winning reservation as game mode and transitions to Playing.</summary>
    private static void ApplyWinnerAndStartPlaying(
        Domain.GameFlow.GameState state,
        IReservation winner
    )
    {
        state.Apply(new SetGameModeModification(winner));
        state.Apply(new AdvancePhaseModification(GamePhase.Playing));
        state.Apply(new SetCurrentTurnModification(state.Players[0].Id));
    }

    /// <summary>
    /// Attempts to set Schlanker Martin as the game mode for the given player;
    /// falls back to normal game if the rules or hand do not allow it.
    /// </summary>
    private static void ApplyFallbackGameMode(
        Domain.GameFlow.GameState state,
        PlayerId martinPlayer
    )
    {
        var playerState = state.Players.First(p => p.Id == martinPlayer);
        var schlankerMartin = new SchlankerMartinReservation(martinPlayer);
        var gameMode = schlankerMartin.IsEligible(playerState.Hand, state.Rules)
            ? (IReservation?)schlankerMartin
            : null;
        state.Apply(new SetGameModeModification(gameMode));
        state.Apply(new AdvancePhaseModification(GamePhase.Playing));
        state.Apply(new SetCurrentTurnModification(state.Players[0].Id));
    }

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
            .ReservationDeclarations.Where(kv => kv.Value?.Priority == target.Priority)
            .OrderBy(kv => (int)state.Players.First(p => p.Id == kv.Key).Seat)
            .First()
            .Key;

    /// <summary>Returns Vorbehalt players in seat order.</summary>
    private static IReadOnlyList<PlayerId> VorbehaltPlayers(Domain.GameFlow.GameState state) =>
        state
            .Players.Where(p => state.HealthDeclarations.TryGetValue(p.Id, out var hasV) && hasV)
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
