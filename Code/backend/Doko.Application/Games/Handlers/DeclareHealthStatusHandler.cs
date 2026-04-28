using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Players;
using Doko.Domain.Sonderkarten;
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
    public async Task<GameActionResult<DeclareHealthStatusResult>> ExecuteAsync(
        DeclareHealthStatusCommand command,
        CancellationToken ct = default
    )
    {
        var loaded = await repository.LoadOrFailAsync<DeclareHealthStatusResult>(
            command.GameId,
            ct
        );
        if (loaded.Failure is not null)
            return loaded.Failure;
        var state = loaded.State!;

        if (state.Phase != GamePhase.ReservationHealthCheck)
            return Fail<DeclareHealthStatusResult>(GameError.InvalidPhase);

        // Must be the player's turn (first in pending queue)
        if (
            state.PendingReservationResponders.Count == 0
            || state.PendingReservationResponders[0] != command.Player
        )
            return Fail<DeclareHealthStatusResult>(GameError.NotYourTurn);

        if (state.HealthDeclarations.ContainsKey(command.Player))
            return Fail<DeclareHealthStatusResult>(GameError.AlreadyDeclared);

        var events = new List<IDomainEvent>
        {
            new HealthDeclaredEvent(state.Id, command.Player, command.HasVorbehalt),
        };

        state.Apply(new RecordHealthDeclarationModification(command.Player, command.HasVorbehalt));

        var remaining = AdvancePendingQueue(state);

        if (remaining.Count > 0)
            return await SaveAndReturn(state, events, new DeclareHealthStatusResult(false), ct);

        ResolveNextPhaseAfterAllDeclared(state);
        return await SaveAndReturn(state, events, new DeclareHealthStatusResult(true), ct);
    }

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
            // No declared reservation — detect silent game modes or fall back to normal game
            var silentMode = DetectSilentMode(state);
            if (silentMode is not null)
                state.Apply(new SetSilentGameModeModification(silentMode));
            else
                state.Apply(new SetGameModeModification(null, null));
            state.Apply(new AdvancePhaseModification(GamePhase.Playing));
            state.Apply(new SetCurrentTurnModification(state.VorbehaltRauskommer));
        }
        else
        {
            // One or more Vorbehalt players → move to Solo check, ordered from VorbehaltRauskommer
            state.Apply(new SetPendingRespondersModification(vorbehaltPlayers));
            state.Apply(new AdvancePhaseModification(GamePhase.ReservationSoloCheck));
            state.Apply(new SetCurrentTurnModification(vorbehaltPlayers[0]));
        }
    }

    private static SilentGameMode? DetectSilentMode(GameState state)
    {
        if (state.InitialHands is null)
            return null;

        var pikDame = new CardType(Suit.Pik, Rank.Dame);
        var pikKoenig = new CardType(Suit.Pik, Rank.Koenig);
        var kreuzDame = new CardType(Suit.Kreuz, Rank.Dame);

        if (state.Rules.AllowKontrasolo)
        {
            foreach (var player in state.Players)
            {
                var hand = state.InitialHands[player.Seat];
                bool hasKontrasolo =
                    hand.Cards.Count(c => c.Type == pikDame) >= 2
                    && hand.Cards.Count(c => c.Type == pikKoenig) >= 2;
                if (hasKontrasolo)
                    return new SilentGameMode(SilentGameModeType.KontraSolo, player.Seat);
            }
        }

        if (state.Rules.AllowStilleHochzeit)
        {
            foreach (var player in state.Players)
            {
                var hand = state.InitialHands[player.Seat];
                if (hand.Cards.Count(c => c.Type == kreuzDame) >= 2)
                    return new SilentGameMode(SilentGameModeType.StilleHochzeit, player.Seat);
            }
        }

        return null;
    }

    private async Task<GameActionResult<DeclareHealthStatusResult>> SaveAndReturn(
        GameState state,
        List<IDomainEvent> events,
        DeclareHealthStatusResult result,
        CancellationToken ct
    )
    {
        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
        return Ok(result);
    }
}
