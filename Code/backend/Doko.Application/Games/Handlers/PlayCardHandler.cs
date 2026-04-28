using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.Extrapunkte;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.GameFlow.Modifications;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Rules;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;
using static Doko.Application.Common.GameActionResultExtensions;

namespace Doko.Application.Games.Handlers;

public interface IPlayCardHandler
{
    Task<GameActionResult<PlayCardResult>> ExecuteAsync(
        PlayCardCommand command,
        CancellationToken ct = default
    );
}

public sealed class PlayCardHandler(
    IGameRepository repository,
    IGameEventPublisher publisher,
    IFinishGameHandler finisher
) : IPlayCardHandler
{
    public Task<GameActionResult<PlayCardResult>> ExecuteAsync(
        PlayCardCommand command,
        CancellationToken ct = default
    ) =>
        GameCommandPipeline.RunAsync<PlayCardResult>(
            repository,
            publisher,
            command.GameId,
            GamePhase.Playing,
            execute: state =>
            {
                if (state.CurrentTurn != command.Player)
                    return (Fail<PlayCardResult>(GameError.NotYourTurn), []);

                var playerState = state.Players.First(p => p.Seat == command.Player);
                var card = playerState.Hand.Cards.FirstOrDefault(c => c.Id == command.Card);
                if (card is null)
                    return (Fail<PlayCardResult>(GameError.IllegalCard), []);

                BeginTrickIfNeeded(state);

                if (
                    !CardPlayValidator.CanPlay(
                        card,
                        playerState.Hand,
                        state.CurrentTrick!,
                        state.TrumpEvaluator
                    )
                )
                    return (Fail<PlayCardResult>(GameError.IllegalCard), []);

                var sonderkarteError = ApplySonderkarten(
                    state,
                    card,
                    command,
                    out var sonderkarteEvents
                );
                if (sonderkarteError.HasValue)
                    return (Fail<PlayCardResult>(sonderkarteError.Value), []);

                PlayCardIntoTrick(state, command.Player, playerState, card);

                var events = BuildEvents(state, command.Player, card, sonderkarteEvents);

                return !state.CurrentTrick!.IsComplete
                    ? AdvanceTurn(state, command.Player, events)
                    : CompleteTrick(state, events);
            },
            ct
        );

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Opens a new trick if none is in progress.
    /// Applies any pending direction flip first, so it takes effect on the lead card.
    /// </summary>
    private static void BeginTrickIfNeeded(GameState state)
    {
        if (state.CurrentTrick is not null)
            return;
        if (state.DirectionFlipPending)
            state.Apply(new ReverseDirectionModification());
        state.Apply(new SetCurrentTrickModification(new Trick()));
    }

    /// <summary>
    /// Validates and activates all sonderkarten requested by the command,
    /// and closes declined windows.
    /// Returns null on success; a <see cref="GameError"/> if validation fails.
    /// </summary>
    private static GameError? ApplySonderkarten(
        GameState state,
        Card card,
        PlayCardCommand command,
        out List<IDomainEvent> events
    )
    {
        events = [];
        var eligible = SonderkarteRegistry.GetEligibleForCard(card, state, state.Rules);
        var eligibleSet = eligible.Select(s => s.Type).ToHashSet();
        var activateSet = command.ActivateSonderkarten.ToHashSet();

        foreach (var type in activateSet)
            if (!eligibleSet.Contains(type))
                return GameError.SonderkarteNotEligible;

        var genscherError = ValidateGenscherIfNeeded(state, command, activateSet);
        if (genscherError.HasValue)
            return genscherError;

        var inputs = new CommandInputProvider(command);

        foreach (var sonderkarte in eligible.Where(s => activateSet.Contains(s.Type)))
        {
            var mods = sonderkarte.Apply(state, inputs);
            foreach (var mod in mods)
                state.Apply(mod);
            events.Add(
                new SonderkarteTriggeredEvent(state.Id, command.Player, sonderkarte.Type, mods)
            );
        }

        foreach (
            var sonderkarte in eligible.Where(s =>
                s.WindowClosesWhenDeclined && !activateSet.Contains(s.Type)
            )
        )
            state.Apply(new CloseActivationWindowModification(sonderkarte.Type));

        return null;
    }

    /// <summary>
    /// When Genscherdamen or Gegengenscherdamen is being activated, validates that the
    /// chosen partner is present and valid. Must run before Apply so errors surface as
    /// <see cref="GameError"/> before any state is mutated.
    /// </summary>
    private static GameError? ValidateGenscherIfNeeded(
        GameState state,
        PlayCardCommand command,
        HashSet<SonderkarteType> activateSet
    )
    {
        bool genscherActivated =
            activateSet.Contains(SonderkarteType.Genscherdamen)
            || activateSet.Contains(SonderkarteType.Gegengenscherdamen);
        if (!genscherActivated)
            return null;

        if (command.GenscherPartner is null)
            return GameError.GenscherPartnerRequired;
        if (command.GenscherPartner == command.Player)
            return GameError.GenscherPartnerInvalid;
        if (state.Players.All(p => p.Seat != command.GenscherPartner))
            return GameError.GenscherPartnerInvalid;

        return null;
    }

    private sealed class CommandInputProvider(PlayCardCommand command) : ISonderkarteInputProvider
    {
        public PlayerSeat GetGenscherPartner() => command.GenscherPartner!.Value;
    }

    private static void PlayCardIntoTrick(
        GameState state,
        PlayerSeat player,
        PlayerState playerState,
        Card card
    )
    {
        state.Apply(new UpdatePlayerHandModification(player, playerState.Hand.Remove(card)));
        state.Apply(new AddCardToTrickModification(player, card));
    }

    private static List<IDomainEvent> BuildEvents(
        GameState state,
        PlayerSeat player,
        Card card,
        List<IDomainEvent> sonderkarteEvents
    ) =>
        [
            .. sonderkarteEvents,
            new CardPlayedEvent(state.Id, player, card, state.CompletedTricks.Count),
        ];

    private static (GameActionResult<PlayCardResult>, IReadOnlyList<IDomainEvent>) AdvanceTurn(
        GameState state,
        PlayerSeat player,
        List<IDomainEvent> events
    )
    {
        state.Apply(new SetCurrentTurnModification(player.Next(state.Direction)));
        return (Ok(new PlayCardResult(false, null, false, null)), events);
    }

    private (GameActionResult<PlayCardResult>, IReadOnlyList<IDomainEvent>) CompleteTrick(
        GameState state,
        List<IDomainEvent> events
    )
    {
        var trick = state.CurrentTrick!;
        var isLastTrick = state.CompletedTricks.Count == state.Rules.LastTrickIndex;
        bool isSchlankerMartin =
            state.ActiveReservation?.Priority == ReservationPriority.SchlankerMartin;
        var dulleRule =
            isSchlankerMartin || isLastTrick
                ? state.Rules.DulleRule.Reversed()
                : state.Rules.DulleRule;
        var normalWinner = trick.Winner(
            state.TrumpEvaluator,
            dulleRule,
            secondBeatsFirst: isSchlankerMartin
        );
        var effectiveWinner = TrickWinnerRuleRegistry.GetEffectiveWinner(
            trick,
            state,
            normalWinner
        );
        var awards = ExtrapunktRegistry
            .GetActive(state.Rules, state.ActiveReservation)
            .SelectMany(e => e.Evaluate(trick, state, effectiveWinner))
            .ToList();
        var result = new TrickResult(trick, effectiveWinner, awards);

        state.Apply(new AddCompletedTrickModification(trick, result));
        events.Add(
            new TrickCompletedEvent(
                state.Id,
                state.CompletedTricks.Count - 1,
                effectiveWinner,
                result
            )
        );

        // Detect Hochzeit forced solo: partner not found after 3 qualifying tricks
        if (ForceIntoSolo(state))
            state.Apply(new SetHochzeitForcedSoloModification());

        // Auto-make Pflichtansage if the completed trick triggers one
        var pflichtAnnouncement = AnnouncementRules.GetMandatoryAnnouncement(
            effectiveWinner,
            state
        );
        if (pflichtAnnouncement is not null)
        {
            state.Apply(new AddAnnouncementModification(pflichtAnnouncement));
            events.Add(
                new AnnouncementMadeEvent(
                    state.Id,
                    pflichtAnnouncement.Player,
                    pflichtAnnouncement.Type,
                    pflichtAnnouncement.TrickNumber,
                    pflichtAnnouncement.CardIndexInTrick
                )
            );
        }

        // Schwarze Sau: when the second ♠Q appears in a completed trick, pause for solo selection.
        // Only interrupt if there are cards left; if hands are empty the game ends normally and
        // scoring falls back to Normalspiel (astronomically unlikely edge case).
        if (
            state.IsSchwarzesSau
            && !state.Players.All(p => p.Hand.Cards.Count == 0)
            && SchwarzesSauTrigger.IsSecondPikDameTrick(state)
        )
        {
            state.Apply(new AdvancePhaseModification(GamePhase.SchwarzesSauSoloSelect));
            state.Apply(new SetCurrentTurnModification(effectiveWinner));
            return (Ok(new PlayCardResult(true, effectiveWinner, false, null)), events);
        }

        if (state.Players.All(p => p.Hand.Cards.Count == 0))
        {
            var finished = finisher.Execute(state);
            return (Ok(new PlayCardResult(true, effectiveWinner, true, finished)), events);
        }

        state.Apply(new SetCurrentTurnModification(effectiveWinner));
        return (Ok(new PlayCardResult(true, effectiveWinner, false, null)), events);
    }

    private static bool ForceIntoSolo(GameState state) =>
        !state.HochzeitBecameForcedSolo
        && state.ActiveReservation is HochzeitReservation
        && state.PartyResolver.IsFullyResolved(state)
        && state.Players.Count(p => state.PartyResolver.ResolveParty(p.Seat, state) == Party.Re)
            == 1;
}
