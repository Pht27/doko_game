using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.Extrapunkte;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Rules;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;

namespace Doko.Application.Games.UseCases;

public interface IPlayCardUseCase
{
    Task<GameActionResult<PlayCardResult>> ExecuteAsync(PlayCardCommand command, CancellationToken ct = default);
}

public sealed class PlayCardUseCase(
    IGameRepository repository,
    IGameEventPublisher publisher,
    IGameScorer scorer) : IPlayCardUseCase
{
    private readonly FinishGameUseCase _finisher = new(scorer);

    public async Task<GameActionResult<PlayCardResult>> ExecuteAsync(PlayCardCommand command, CancellationToken ct = default)
    {
        var state = await repository.GetAsync(command.GameId, ct);
        if (state is null)
            return new GameActionResult<PlayCardResult>.Failure(GameError.GameNotFound);

        if (state.Phase != GamePhase.Playing)
            return new GameActionResult<PlayCardResult>.Failure(GameError.InvalidPhase);

        if (state.CurrentTurn != command.Player)
            return new GameActionResult<PlayCardResult>.Failure(GameError.NotYourTurn);

        var playerState = state.Players.First(p => p.Id == command.Player);
        var card = playerState.Hand.Cards.FirstOrDefault(c => c.Id == command.Card);
        if (card is null)
            return new GameActionResult<PlayCardResult>.Failure(GameError.IllegalCard);

        // Ensure there is a current trick (start one if needed)
        var currentTrick = state.CurrentTrick;
        if (currentTrick is null)
        {
            currentTrick = new Trick();
            state.Apply(new SetCurrentTrickModification(currentTrick));
        }

        // Validate follow-suit rule
        if (!CardPlayValidator.CanPlay(card, playerState.Hand, currentTrick, state.TrumpEvaluator))
            return new GameActionResult<PlayCardResult>.Failure(GameError.IllegalCard);

        // Validate and apply sonderkarte activations
        var eligible = SonderkarteRegistry.GetEligibleForCard(card, state, state.Rules);
        var eligibleTypes = eligible.Select(s => s.Type).ToHashSet();

        foreach (var type in command.ActivateSonderkarten)
        {
            if (!eligibleTypes.Contains(type))
                return new GameActionResult<PlayCardResult>.Failure(GameError.SonderkarteNotEligible);
        }

        var activateSet = command.ActivateSonderkarten.ToHashSet();
        var sonderkarteEvents = new List<IDomainEvent>();

        foreach (var sonderkarte in eligible.Where(s => activateSet.Contains(s.Type)))
        {
            var mod = sonderkarte.Apply(state);
            if (mod is not null)
                state.Apply(mod);
            sonderkarteEvents.Add(new SonderkarteTriggeredEvent(state.Id, command.Player, sonderkarte.Type, mod));
        }

        // Remove card from player's hand
        var newHand = playerState.Hand.Remove(card);
        state.Apply(new UpdatePlayerHandModification(command.Player, newHand));

        // Add card to trick
        currentTrick.Add(new TrickCard(card, command.Player));

        var events = new List<IDomainEvent>(sonderkarteEvents)
        {
            new CardPlayedEvent(state.Id, command.Player, card, state.CompletedTricks.Count),
        };

        if (!currentTrick.IsComplete)
        {
            // Advance turn to next player
            var next = state.NextPlayer(command.Player, state.Direction);
            state.Apply(new SetCurrentTurnModification(next));

            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, events, ct);

            return new GameActionResult<PlayCardResult>.Ok(
                new PlayCardResult(false, null, false, null));
        }

        // Trick complete — compute winner and extrapunkts
        var winner = currentTrick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
        var awards = ExtrapunktRegistry.GetActive(state.Rules)
            .SelectMany(e => e.Evaluate(currentTrick, state))
            .ToList();

        var trickResult = new TrickResult(currentTrick, winner, awards);
        state.Apply(new AddCompletedTrickModification(currentTrick, trickResult));

        events.Add(new TrickCompletedEvent(state.Id, state.CompletedTricks.Count - 1, winner, trickResult));

        // Check for party reveals (Hochzeit partner discovered, etc.)
        // PartyResolver handles this implicitly; surface KnownParty changes as events if needed

        bool gameFinished = state.Players.All(p => p.Hand.Cards.Count == 0);

        if (gameFinished)
        {
            var finished = _finisher.Execute(state);

            await repository.SaveAsync(state, ct);
            await publisher.PublishAsync(state.Id, events, ct);

            return new GameActionResult<PlayCardResult>.Ok(
                new PlayCardResult(true, winner, true, finished));
        }

        // Set winner as the player to lead the next trick
        state.Apply(new SetCurrentTurnModification(winner));

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);

        return new GameActionResult<PlayCardResult>.Ok(
            new PlayCardResult(true, winner, false, null));
    }
}
