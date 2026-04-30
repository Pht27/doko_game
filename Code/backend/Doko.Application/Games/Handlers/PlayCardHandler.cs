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
        GameCommandPipeline.RunAsync<PlayCardResult, PlayingState>(
            repository,
            publisher,
            command.GameId,
            execute: (PlayingState state) =>
            {
                if (state.Phase != GamePhase.Playing)
                    return (Fail<PlayCardResult>(GameError.InvalidPhase), [], state);

                if (state.CurrentTurn != command.Player)
                    return (Fail<PlayCardResult>(GameError.NotYourTurn), [], state);

                var playerState = state.Players.First(p => p.Seat == command.Player);
                var card = playerState.Hand.Cards.FirstOrDefault(c => c.Id == command.Card);
                if (card is null)
                    return (Fail<PlayCardResult>(GameError.IllegalCard), [], state);

                state = BeginTrickIfNeeded(state);

                if (
                    !CardPlayValidator.CanPlay(
                        card,
                        playerState.Hand,
                        state.CurrentTrick!,
                        state.TrumpEvaluator
                    )
                )
                    return (Fail<PlayCardResult>(GameError.IllegalCard), [], state);

                var (sonderkarteError, sonderkarteEvents, stateAfterSonderkarten) =
                    ApplySonderkarten(state, card, command);
                if (sonderkarteError.HasValue)
                    return (Fail<PlayCardResult>(sonderkarteError.Value), [], state);
                state = stateAfterSonderkarten;

                state = PlayCardIntoTrick(state, command.Player, playerState, card);

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
    private static PlayingState BeginTrickIfNeeded(PlayingState state)
    {
        if (state.CurrentTrick is not null)
            return state;
        if (state.DirectionFlipPending)
            state = (PlayingState)state.Apply(new ReverseDirectionModification());
        return (PlayingState)state.Apply(new SetCurrentTrickModification(new Trick()));
    }

    /// <summary>
    /// Validates and activates all sonderkarten requested by the command,
    /// and closes declined windows.
    /// Returns null error on success; a <see cref="GameError"/> if validation fails.
    /// </summary>
    private static (
        GameError? error,
        List<IDomainEvent> events,
        PlayingState nextState
    ) ApplySonderkarten(PlayingState state, Card card, PlayCardCommand command)
    {
        var eligible = SonderkarteRegistry.GetEligibleForCard(card, state, state.Rules);
        var eligibleSet = eligible.Select(s => s.Type).ToHashSet();
        var activateSet = command.ActivateSonderkarten.ToHashSet();

        foreach (var type in activateSet)
            if (!eligibleSet.Contains(type))
                return (GameError.SonderkarteNotEligible, [], state);

        var genscherError = ValidateGenscherIfNeeded(state, command, activateSet);
        if (genscherError.HasValue)
            return (genscherError, [], state);

        var inputs = new CommandInputProvider(command);
        var events = new List<IDomainEvent>();

        foreach (var sonderkarte in eligible.Where(s => activateSet.Contains(s.Type)))
        {
            var mods = sonderkarte.Apply(state, inputs);
            foreach (var mod in mods)
                state = (PlayingState)state.Apply(mod);
            events.Add(
                new SonderkarteTriggeredEvent(state.Id, command.Player, sonderkarte.Type, mods)
            );
        }

        foreach (
            var sonderkarte in eligible.Where(s =>
                s.WindowClosesWhenDeclined && !activateSet.Contains(s.Type)
            )
        )
            state = (PlayingState)state.Apply(new CloseActivationWindowModification(sonderkarte.Type));

        return (null, events, state);
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

    private static PlayingState PlayCardIntoTrick(
        PlayingState state,
        PlayerSeat player,
        PlayerState playerState,
        Card card
    )
    {
        state = (PlayingState)state.Apply(
            new UpdatePlayerHandModification(player, playerState.Hand.Remove(card))
        );
        return (PlayingState)state.Apply(new AddCardToTrickModification(player, card));
    }

    private static List<IDomainEvent> BuildEvents(
        PlayingState state,
        PlayerSeat player,
        Card card,
        List<IDomainEvent> sonderkarteEvents
    ) =>
        [
            .. sonderkarteEvents,
            new CardPlayedEvent(state.Id, player, card, state.CompletedTricks.Count),
        ];

    private static (
        GameActionResult<PlayCardResult>,
        IReadOnlyList<IDomainEvent>,
        GameState
    ) AdvanceTurn(PlayingState state, PlayerSeat player, List<IDomainEvent> events)
    {
        state = (PlayingState)state.Apply(new SetCurrentTurnModification(player.Next(state.Direction)));
        return (Ok(new PlayCardResult(false, null, false, null)), events, state);
    }

    private (
        GameActionResult<PlayCardResult>,
        IReadOnlyList<IDomainEvent>,
        GameState
    ) CompleteTrick(PlayingState state, List<IDomainEvent> events)
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

        state = (PlayingState)state.Apply(new AddCompletedTrickModification(trick, result));
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
            state = (PlayingState)state.Apply(new SetHochzeitForcedSoloModification());

        // Auto-make Pflichtansage if the completed trick triggers one
        var pflichtAnnouncement = AnnouncementRules.GetMandatoryAnnouncement(
            effectiveWinner,
            state
        );
        if (pflichtAnnouncement is not null)
        {
            state = (PlayingState)state.Apply(new AddAnnouncementModification(pflichtAnnouncement));
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
            GameState nextState = state.Apply(new AdvancePhaseModification(GamePhase.SchwarzesSauSoloSelect));
            nextState = nextState.Apply(new SetCurrentTurnModification(effectiveWinner));
            return (Ok(new PlayCardResult(true, effectiveWinner, false, null)), events, nextState);
        }

        if (state.Players.All(p => p.Hand.Cards.Count == 0))
        {
            var (finished, nextState) = finisher.Execute(state);
            return (
                Ok(new PlayCardResult(true, effectiveWinner, true, finished)),
                events,
                nextState
            );
        }

        state = (PlayingState)state.Apply(new SetCurrentTurnModification(effectiveWinner));
        return (Ok(new PlayCardResult(true, effectiveWinner, false, null)), events, state);
    }

    private static bool ForceIntoSolo(PlayingState state) =>
        !state.HochzeitBecameForcedSolo
        && state.ActiveReservation is HochzeitReservation
        && state.PartyResolver.IsFullyResolved(state)
        && state.Players.Count(p => state.PartyResolver.ResolveParty(p.Seat, state) == Party.Re)
            == 1;
}
