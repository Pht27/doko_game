using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.GameFlow.Modifications;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using static Doko.Application.Common.GameActionResultExtensions;

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
        var loaded = await repository.LoadOrFailAsync<MakeReservationResult>(command.GameId, ct);
        if (loaded.Failure is not null)
            return loaded.Failure;
        if (loaded.State is not ReservationState reservationState)
            return Fail<MakeReservationResult>(GameError.InvalidPhase);

        var validationError = Validate(command, reservationState);
        if (validationError is not null)
            return Fail<MakeReservationResult>(validationError.Value);

        var events = new List<IDomainEvent>
        {
            new ReservationMadeEvent(reservationState.Id, command.Player, command.Reservation),
        };

        reservationState = RecordAndAdvanceQueue(command, reservationState);

        if (reservationState.PendingReservationResponders.Count > 0)
            return await SaveAndReturnOk(reservationState, events, new MakeReservationResult(false, null), ct);

        return await ResolvePhaseAsync(reservationState, events, ct);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    /// <summary>Returns the first validation error for the command, or null if valid.</summary>
    private static GameError? Validate(MakeReservationCommand command, ReservationState state)
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
    private static ReservationState RecordAndAdvanceQueue(MakeReservationCommand command, ReservationState state)
    {
        state = (ReservationState)state.Apply(new RecordDeclarationModification(command.Player, command.Reservation));
        var remaining = state.PendingReservationResponders.Skip(1).ToList();
        state = (ReservationState)state.Apply(new SetPendingRespondersModification(remaining));
        if (remaining.Count > 0)
            state = (ReservationState)state.Apply(new SetCurrentTurnModification(remaining[0]));
        return state;
    }

    // ── Per-phase declaration validation ──────────────────────────────────────

    private static bool IsAllowedInPhase(
        IReservation? reservation,
        GamePhase phase,
        ReservationState state
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
        ReservationState state,
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
        ReservationState state,
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
            var playingState = ApplyWinnerAndStartPlaying(state, winner);
            return await SaveAndReturnOk(
                playingState,
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
        ReservationState state,
        List<IDomainEvent> events,
        CancellationToken ct
    )
    {
        var winner = PickWinner(state);

        if (winner is ArmutReservation)
        {
            // Find which player declared Armut (first in order = lowest seat)
            var armutPlayerSeat = FirstDeclarantId(state, winner);
            GameState nextState = state.Apply(new SetArmutPlayerModification(armutPlayerSeat));

            // Determine partner-finding queue: players after poor player in seat order
            var partnerCandidates = PlayersAfter(state, armutPlayerSeat);
            nextState = nextState.Apply(new SetPendingRespondersModification(partnerCandidates));
            nextState = nextState.Apply(new AdvancePhaseModification(GamePhase.ArmutPartnerFinding));
            nextState = nextState.Apply(new SetCurrentTurnModification(partnerCandidates[0]));

            return await SaveAndReturnOk(
                nextState,
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
        ReservationState state,
        List<IDomainEvent> events,
        CancellationToken ct
    )
    {
        var winner = PickWinner(state);

        if (winner is SchmeissenReservation)
        {
            GameState nextState = state.Apply(new AdvancePhaseModification(GamePhase.Geschmissen));
            return await SaveAndReturnOk(
                nextState,
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
        ReservationState state,
        List<IDomainEvent> events,
        CancellationToken ct
    )
    {
        var winner = PickWinner(state);

        if (winner is HochzeitReservation hochzeit)
        {
            var playingState = ApplyWinnerAndStartPlaying(state, hochzeit);
            return await SaveAndReturnOk(
                playingState,
                events,
                new MakeReservationResult(true, hochzeit),
                ct
            );
        }

        // No Hochzeit — force Schlanker Martin for the first Vorbehalt player, or normal game
        var martinPlayer = VorbehaltPlayers(state)[0];
        var (fallbackMode, fallbackState) = ApplyFallbackGameMode(state, martinPlayer);
        return await SaveAndReturnOk(
            fallbackState,
            events,
            new MakeReservationResult(true, fallbackMode),
            ct
        );
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Saves state, publishes events, and returns Ok with the given result.</summary>
    private async Task<GameActionResult<MakeReservationResult>> SaveAndReturnOk(
        GameState state,
        List<IDomainEvent> events,
        MakeReservationResult result,
        CancellationToken ct
    )
    {
        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
        return Ok(result);
    }

    /// <summary>Sets the winning reservation as game mode and transitions to Playing.</summary>
    private static GameState ApplyWinnerAndStartPlaying(ReservationState state, IReservation winner)
    {
        var declarant = FirstDeclarantId(state, winner);
        // Read VorbehaltRauskommer before phase transition (it won't be on PlayingState)
        var vorbehaltRauskommer = state.VorbehaltRauskommer;
        GameState nextState = state.Apply(new SetGameModeModification(winner, declarant));
        nextState = nextState.Apply(new AdvancePhaseModification(GamePhase.Playing));
        // Solo declarer leads in Solo games; VorbehaltRauskommer leads otherwise.
        var spieleRauskommer = IsSoloReservation(winner) ? declarant : vorbehaltRauskommer;
        nextState = nextState.Apply(new SetCurrentTurnModification(spieleRauskommer));
        return nextState;
    }

    /// <summary>
    /// Attempts to set Schlanker Martin as the game mode for the given player;
    /// falls back to normal game if the rules or hand do not allow it.
    /// </summary>
    private static (IReservation? gameMode, GameState nextState) ApplyFallbackGameMode(ReservationState state, PlayerSeat martinPlayer)
    {
        var playerState = state.Players.First(p => p.Seat == martinPlayer);
        var schlankerMartin = new SchlankerMartinReservation(martinPlayer);
        var gameMode = schlankerMartin.IsEligible(playerState.Hand, state.Rules)
            ? (IReservation?)schlankerMartin
            : null;
        GameState nextState = state.Apply(
            new SetGameModeModification(gameMode, gameMode != null ? martinPlayer : null)
        );
        nextState = nextState.Apply(new AdvancePhaseModification(GamePhase.Playing));
        nextState = nextState.Apply(new SetCurrentTurnModification(martinPlayer));
        return (gameMode, nextState);
    }

    /// <summary>
    /// Picks the highest-priority winner from current declarations (lowest Priority value).
    /// Tie-breaks by the first player in seat order.
    /// </summary>
    private static IReservation? PickWinner(ReservationState state) =>
        state
            .ReservationDeclarations.Where(kv => kv.Value is not null)
            .OrderBy(kv => kv.Value!.Priority)
            .ThenBy(kv => (int)kv.Key)
            .Select(kv => kv.Value)
            .FirstOrDefault();

    /// <summary>Returns the player ID of the first declarant of the given reservation (by seat order).</summary>
    private static PlayerSeat FirstDeclarantId(ReservationState state, IReservation target) =>
        state
            .ReservationDeclarations.Where(kv => kv.Value?.Priority == target.Priority)
            .OrderBy(kv => (int)kv.Key)
            .First()
            .Key;

    /// <summary>Returns Vorbehalt players in seat order.</summary>
    private static IReadOnlyList<PlayerSeat> VorbehaltPlayers(ReservationState state) =>
        state
            .Players.Where(p => state.HealthDeclarations.TryGetValue(p.Seat, out var hasV) && hasV)
            .Select(p => p.Seat)
            .ToList();

    /// <summary>Returns players seated after <paramref name="player"/> (wrapping around), in seat order.</summary>
    private static IReadOnlyList<PlayerSeat> PlayersAfter(GameState state, PlayerSeat player)
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
        ReservationState state,
        List<IDomainEvent> events,
        GamePhase next,
        CancellationToken ct
    )
    {
        var eligible = EligiblePlayersForPhase(state, next);
        ReservationState nextState = (ReservationState)state.Apply(new ClearReservationDeclarationsModification());
        nextState = (ReservationState)nextState.Apply(new AdvancePhaseModification(next));

        if (eligible.Count > 0)
        {
            nextState = (ReservationState)nextState.Apply(new SetPendingRespondersModification(eligible));
            nextState = (ReservationState)nextState.Apply(new SetCurrentTurnModification(eligible[0]));
            return await SaveAndReturnOk(nextState, events, new MakeReservationResult(false, null), ct);
        }

        // No eligible players — resolve the phase immediately with empty declarations
        nextState = (ReservationState)nextState.Apply(new SetPendingRespondersModification([]));
        return await ResolvePhaseAsync(nextState, events, ct);
    }

    /// <summary>
    /// Returns the Vorbehalt players who are eligible to declare the reservation type for
    /// the given phase. For SoloCheck the full Vorbehalt list is returned (no filtering).
    /// </summary>
    private static IReadOnlyList<PlayerSeat> EligiblePlayersForPhase(
        ReservationState state,
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
