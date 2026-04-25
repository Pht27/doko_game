using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.Handlers;

public interface IMakeReservationHandler
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
public sealed class MakeReservationHandler(
    IGameRepository repository,
    IGameEventPublisher publisher
) : IMakeReservationHandler
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

        var playerState = state.Players.First(p => p.Seat == command.Player);
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

        // No Solo — advance to Armut check (skipping players not eligible for Armut)
        return await AdvanceToNextCheckPhaseOrResolveAsync(
            state,
            events,
            GamePhase.ReservationArmutCheck,
            ct
        );
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
            var armutPlayerSeat = FirstDeclarantId(state, winner);
            state.Apply(new SetArmutPlayerModification(armutPlayerSeat));

            // Determine partner-finding queue: players after poor player in seat order
            var partnerCandidates = PlayersAfter(state, armutPlayerSeat);
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

        // No Armut — advance to Schmeißen check (skipping players not eligible for Schmeißen)
        return await AdvanceToNextCheckPhaseOrResolveAsync(
            state,
            events,
            GamePhase.ReservationSchmeissenCheck,
            ct
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
            return await SaveAndReturnOk(
                state,
                events,
                new MakeReservationResult(true, winner, Geschmissen: true),
                ct
            );
        }

        // No Schmeißen — advance to Hochzeit check (skipping players not eligible for Hochzeit)
        return await AdvanceToNextCheckPhaseOrResolveAsync(
            state,
            events,
            GamePhase.ReservationHochzeitCheck,
            ct
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
        // Solo declarer leads in Solo games; VorbehaltRauskommer leads otherwise.
        var spieleRauskommer = IsSoloReservation(winner)
            ? FirstDeclarantId(state, winner)
            : state.VorbehaltRauskommer;
        state.Apply(new SetCurrentTurnModification(spieleRauskommer));
    }

    /// <summary>
    /// Attempts to set Schlanker Martin as the game mode for the given player;
    /// falls back to normal game if the rules or hand do not allow it.
    /// </summary>
    private static void ApplyFallbackGameMode(
        Domain.GameFlow.GameState state,
        PlayerSeat martinPlayer
    )
    {
        var playerState = state.Players.First(p => p.Seat == martinPlayer);
        var schlankerMartin = new SchlankerMartinReservation(martinPlayer);
        var gameMode = schlankerMartin.IsEligible(playerState.Hand, state.Rules)
            ? (IReservation?)schlankerMartin
            : null;
        state.Apply(new SetGameModeModification(gameMode));
        state.Apply(new AdvancePhaseModification(GamePhase.Playing));
        state.Apply(new SetCurrentTurnModification(state.VorbehaltRauskommer));
    }

    /// <summary>
    /// Picks the highest-priority winner from current declarations (lowest Priority value).
    /// Tie-breaks by the first player in seat order.
    /// </summary>
    private static IReservation? PickWinner(Domain.GameFlow.GameState state) =>
        state
            .ReservationDeclarations.Where(kv => kv.Value is not null)
            .OrderBy(kv => kv.Value!.Priority)
            .ThenBy(kv => (int)kv.Key)
            .Select(kv => kv.Value)
            .FirstOrDefault();

    /// <summary>Returns the player ID of the first declarant of the given reservation (by seat order).</summary>
    private static PlayerSeat FirstDeclarantId(
        Domain.GameFlow.GameState state,
        IReservation target
    ) =>
        state
            .ReservationDeclarations.Where(kv => kv.Value?.Priority == target.Priority)
            .OrderBy(kv => (int)kv.Key)
            .First()
            .Key;

    /// <summary>Returns Vorbehalt players in seat order.</summary>
    private static IReadOnlyList<PlayerSeat> VorbehaltPlayers(Domain.GameFlow.GameState state) =>
        state
            .Players.Where(p => state.HealthDeclarations.TryGetValue(p.Seat, out var hasV) && hasV)
            .Select(p => p.Seat)
            .ToList();

    /// <summary>Returns players seated after <paramref name="player"/> (wrapping around), in seat order.</summary>
    private static IReadOnlyList<PlayerSeat> PlayersAfter(
        Domain.GameFlow.GameState state,
        PlayerSeat player
    )
    {
        int seat = (int)player;
        return state
            .Players.OrderBy(p => ((int)p.Seat - seat + 4) % 4)
            .Skip(1) // exclude the player themselves
            .Select(p => p.Seat)
            .ToList();
    }

    /// <summary>
    /// Advances to the next check phase, but only queues players who are eligible for that
    /// phase's reservation type. If no eligible players remain, the phase is resolved immediately
    /// (everyone auto-passes), which may cascade further until a winner is found or the game starts.
    /// </summary>
    private async Task<
        GameActionResult<MakeReservationResult>
    > AdvanceToNextCheckPhaseOrResolveAsync(
        Domain.GameFlow.GameState state,
        List<IDomainEvent> events,
        GamePhase next,
        CancellationToken ct
    )
    {
        var eligible = EligiblePlayersForPhase(state, next);
        state.Apply(new ClearReservationDeclarationsModification());
        state.Apply(new AdvancePhaseModification(next));

        if (eligible.Count > 0)
        {
            state.Apply(new SetPendingRespondersModification(eligible));
            state.Apply(new SetCurrentTurnModification(eligible[0]));
            return await SaveAndReturnOk(state, events, new MakeReservationResult(false, null), ct);
        }

        // No eligible players — resolve the phase immediately with empty declarations
        state.Apply(new SetPendingRespondersModification([]));
        return await ResolvePhaseAsync(state, events, ct);
    }

    /// <summary>
    /// Returns the Vorbehalt players who are eligible to declare the reservation type for
    /// the given phase. For SoloCheck the full Vorbehalt list is returned (no filtering).
    /// </summary>
    private static IReadOnlyList<PlayerSeat> EligiblePlayersForPhase(
        Domain.GameFlow.GameState state,
        GamePhase phase
    )
    {
        var vorbehalt = VorbehaltPlayers(state);
        return phase switch
        {
            GamePhase.ReservationArmutCheck =>
            [
                .. vorbehalt.Where(seat =>
                {
                    var ps = state.Players.First(p => p.Seat == seat);
                    var dummyPartner = (PlayerSeat)(((int)seat + 1) % 4);
                    return new ArmutReservation(seat, dummyPartner).IsEligible(
                        ps.Hand,
                        state.Rules
                    );
                }),
            ],
            GamePhase.ReservationSchmeissenCheck =>
            [
                .. vorbehalt.Where(seat =>
                {
                    var ps = state.Players.First(p => p.Seat == seat);
                    return new SchmeissenReservation().IsEligible(ps.Hand, state.Rules);
                }),
            ],
            GamePhase.ReservationHochzeitCheck =>
            [
                .. vorbehalt.Where(seat =>
                {
                    var ps = state.Players.First(p => p.Seat == seat);
                    return new HochzeitReservation(seat, HochzeitCondition.FirstTrick).IsEligible(
                        ps.Hand,
                        state.Rules
                    );
                }),
            ],
            _ => vorbehalt,
        };
    }
}
