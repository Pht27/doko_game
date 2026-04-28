using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;
using static Doko.Application.Common.GameActionResultExtensions;

namespace Doko.Application.Games.Handlers;

public interface IChooseSchwarzesSauSoloHandler
{
    Task<GameActionResult<ChooseSchwarzesSauSoloResult>> ExecuteAsync(
        ChooseSchwarzesSauSoloCommand command,
        CancellationToken ct = default
    );
}

/// <summary>
/// Handles the trick winner's solo selection during <see cref="GamePhase.SchwarzesSauSoloSelect"/>.
/// The chosen solo's trump evaluator and party resolver are applied immediately; any previously
/// active sonderkarte trump effects fall off (same as in the normal reservation flow).
/// Scoring then treats the entire game as if that solo was played from the start.
/// </summary>
public sealed class ChooseSchwarzesSauSoloHandler(
    IGameRepository repository,
    IGameEventPublisher publisher,
    IGameScorer scorer
) : IChooseSchwarzesSauSoloHandler
{
    private readonly FinishGameHandler _finisher = new(scorer);

    public async Task<GameActionResult<ChooseSchwarzesSauSoloResult>> ExecuteAsync(
        ChooseSchwarzesSauSoloCommand command,
        CancellationToken ct = default
    )
    {
        var loaded = await repository.LoadOrFailAsync<ChooseSchwarzesSauSoloResult>(
            command.GameId,
            ct
        );
        if (loaded.Failure is not null)
            return loaded.Failure;
        var state = loaded.State!;

        if (state.Phase != GamePhase.SchwarzesSauSoloSelect)
            return Fail<ChooseSchwarzesSauSoloResult>(GameError.InvalidPhase);

        if (state.CurrentTurn != command.Player)
            return Fail<ChooseSchwarzesSauSoloResult>(GameError.NotYourTurn);

        if (!IsEligibleSolo(command.Solo))
            return Fail<ChooseSchwarzesSauSoloResult>(GameError.ReservationNotEligible);

        var reservation = CreateReservation(command.Solo, command.Player);

        // Apply the solo's trump evaluator and party resolver.
        state.Apply(new SetGameModeModification(reservation, command.Player));

        // Sonderkarte handling:
        // - Non-Schlanker-Martin solos change the trump evaluator completely, so previously-active
        //   sonderkarten are irrelevant and must be cleared.
        // - Schlanker Martin keeps NormalTrumpEvaluator, so active sonderkarten remain valid;
        //   rebuild to re-apply their modifiers on top of the (unchanged) base evaluator.
        if (command.Solo == ReservationPriority.SchlankerMartin)
            state.Apply(new RebuildTrumpEvaluatorModification());
        else
            state.Apply(new ClearActiveSonderkartenModification());

        // Announcements from the pre-solo Normalspiel phase are always discarded —
        // they were made under the wrong party structure.
        state.Apply(new ClearAnnouncementsModification());

        // Extrapunkte earned during the pre-solo phase are invalidated.
        state.Apply(new ClearScoredTrickAwardsModification());

        state.Apply(new AdvancePhaseModification(GamePhase.Playing));
        state.Apply(new SetCurrentTurnModification(command.Player));

        // Edge case: all hands empty means the second ♠Q was in the very last trick.
        // Finish the game immediately with the chosen solo for scoring.
        if (state.Players.All(p => p.Hand.Cards.Count == 0))
        {
            var finished = _finisher.Execute(state);
            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, [], ct);
            return Ok(new ChooseSchwarzesSauSoloResult(finished));
        }

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, [], ct);
        return Ok(new ChooseSchwarzesSauSoloResult(null));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>All solo priorities (KaroSolo=0 … SchlankerMartin=8) are eligible.
    /// Armut, Hochzeit, and Schmeißen are not solos and may not be chosen.</summary>
    private static bool IsEligibleSolo(ReservationPriority priority) =>
        (int)priority <= (int)ReservationPriority.SchlankerMartin;

    /// <summary>Constructs the concrete <see cref="IReservation"/> for the chosen priority.</summary>
    private static IReservation CreateReservation(
        ReservationPriority priority,
        PlayerSeat player
    ) =>
        priority switch
        {
            ReservationPriority.KaroSolo => new FarbsoloReservation(Suit.Karo, player),
            ReservationPriority.KreuzSolo => new FarbsoloReservation(Suit.Kreuz, player),
            ReservationPriority.PikSolo => new FarbsoloReservation(Suit.Pik, player),
            ReservationPriority.HerzSolo => new FarbsoloReservation(Suit.Herz, player),
            ReservationPriority.Damensolo => new DamensoloReservation(player),
            ReservationPriority.Bubensolo => new BubensoloReservation(player),
            ReservationPriority.Fleischloses => new FleischlosesReservation(player),
            ReservationPriority.Knochenloses => new KnochenlosesReservation(player),
            ReservationPriority.SchlankerMartin => new SchlankerMartinReservation(player),
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
        };
}
