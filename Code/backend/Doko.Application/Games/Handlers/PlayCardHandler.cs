using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.Extrapunkte;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Players;
using Doko.Domain.Rules;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;

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
    IGameScorer scorer
) : IPlayCardHandler
{
    private readonly FinishGameHandler _finisher = new(scorer);

    public async Task<GameActionResult<PlayCardResult>> ExecuteAsync(
        PlayCardCommand command,
        CancellationToken ct = default
    )
    {
        var state = await repository.GetAsync(command.GameId, ct);
        if (state is null)
            return Fail(GameError.GameNotFound);
        if (state.Phase != GamePhase.Playing)
            return Fail(GameError.InvalidPhase);
        if (state.CurrentTurn != command.Player)
            return Fail(GameError.NotYourTurn);

        var playerState = state.Players.First(p => p.Id == command.Player);
        var card = playerState.Hand.Cards.FirstOrDefault(c => c.Id == command.Card);
        if (card is null)
            return Fail(GameError.IllegalCard);

        BeginTrickIfNeeded(state);

        if (
            !CardPlayValidator.CanPlay(
                card,
                playerState.Hand,
                state.CurrentTrick!,
                state.TrumpEvaluator
            )
        )
            return Fail(GameError.IllegalCard);

        var sonderkarteError = ApplySonderkarten(state, card, command, out var sonderkarteEvents);
        if (sonderkarteError.HasValue)
            return Fail(sonderkarteError.Value);

        PlayCardIntoTrick(state, command.Player, playerState, card);

        var events = BuildEvents(state, command.Player, card, sonderkarteEvents);

        if (!state.CurrentTrick!.IsComplete)
            return await AdvanceTurnAsync(state, command.Player, events, ct);

        return await CompleteTrickAsync(state, events, ct);
    }

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
        if (state.Players.All(p => p.Id != command.GenscherPartner))
            return GameError.GenscherPartnerInvalid;

        return null;
    }

    private sealed class CommandInputProvider(PlayCardCommand command) : ISonderkarteInputProvider
    {
        public PlayerId GetGenscherPartner() => command.GenscherPartner!.Value;
    }

    private static void PlayCardIntoTrick(
        GameState state,
        PlayerId player,
        PlayerState playerState,
        Card card
    )
    {
        state.Apply(new UpdatePlayerHandModification(player, playerState.Hand.Remove(card)));
        state.Apply(new AddCardToTrickModification(player, card));
    }

    private static List<IDomainEvent> BuildEvents(
        GameState state,
        PlayerId player,
        Card card,
        List<IDomainEvent> sonderkarteEvents
    ) =>
        [
            .. sonderkarteEvents,
            new CardPlayedEvent(state.Id, player, card, state.CompletedTricks.Count),
        ];

    private async Task<GameActionResult<PlayCardResult>> AdvanceTurnAsync(
        GameState state,
        PlayerId player,
        List<IDomainEvent> events,
        CancellationToken ct
    )
    {
        state.Apply(new SetCurrentTurnModification(state.NextPlayer(player, state.Direction)));
        await SaveAndPublishAsync(state, events, ct);
        return Ok(new PlayCardResult(false, null, false, null));
    }

    private async Task<GameActionResult<PlayCardResult>> CompleteTrickAsync(
        GameState state,
        List<IDomainEvent> events,
        CancellationToken ct
    )
    {
        var trick = state.CurrentTrick!;
        var isLastTrick = state.CompletedTricks.Count == state.Rules.LastTrickIndex;
        var dulleRule = isLastTrick ? DulleRule.FirstBeatsSecond : state.Rules.DulleRule;
        var winner = trick.Winner(state.TrumpEvaluator, dulleRule);
        var awards = ExtrapunktRegistry
            .GetActive(state.Rules, state.ActiveReservation)
            .SelectMany(e => e.Evaluate(trick, state))
            .ToList();
        var result = new TrickResult(trick, winner, awards);

        state.Apply(new AddCompletedTrickModification(trick, result));
        events.Add(
            new TrickCompletedEvent(state.Id, state.CompletedTricks.Count - 1, winner, result)
        );

        // Auto-make Pflichtansage if the completed trick triggers one
        var pflichtAnnouncement = AnnouncementRules.GetMandatoryAnnouncement(winner, state);
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

        if (state.Players.All(p => p.Hand.Cards.Count == 0))
        {
            var finished = _finisher.Execute(state);
            await SaveAndPublishAsync(state, events, ct);
            return Ok(new PlayCardResult(true, winner, true, finished));
        }

        state.Apply(new SetCurrentTurnModification(winner));
        await SaveAndPublishAsync(state, events, ct);
        return Ok(new PlayCardResult(true, winner, false, null));
    }

    private async Task SaveAndPublishAsync(
        GameState state,
        List<IDomainEvent> events,
        CancellationToken ct
    )
    {
        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
    }

    private static GameActionResult<PlayCardResult> Ok(PlayCardResult value) =>
        new GameActionResult<PlayCardResult>.Ok(value);

    private static GameActionResult<PlayCardResult> Fail(GameError error) =>
        new GameActionResult<PlayCardResult>.Failure(error);
}
