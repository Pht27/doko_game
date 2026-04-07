using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Hands;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.UseCases;

public interface IExchangeArmutCardsUseCase
{
    Task<GameActionResult<ExchangeArmutCardsResult>> ExecuteAsync(
        ExchangeArmutCardsCommand command,
        CancellationToken ct = default
    );
}

/// <summary>
/// The rich player returns exactly <c>ArmutTransferCount</c> cards to the poor player
/// during <see cref="GamePhase.ArmutCardExchange"/>.
/// </summary>
public sealed class ExchangeArmutCardsUseCase(
    IGameRepository repository,
    IGameEventPublisher publisher
) : IExchangeArmutCardsUseCase
{
    public async Task<GameActionResult<ExchangeArmutCardsResult>> ExecuteAsync(
        ExchangeArmutCardsCommand command,
        CancellationToken ct = default
    )
    {
        var state = await repository.GetAsync(command.GameId, ct);
        if (state is null)
            return new GameActionResult<ExchangeArmutCardsResult>.Failure(GameError.GameNotFound);

        if (state.Phase != GamePhase.ArmutCardExchange)
            return new GameActionResult<ExchangeArmutCardsResult>.Failure(GameError.InvalidPhase);

        if (state.ArmutRichPlayer != command.RichPlayer)
            return new GameActionResult<ExchangeArmutCardsResult>.Failure(GameError.NotYourTurn);

        if (command.CardIdsToReturn.Count != state.ArmutTransferCount)
            return new GameActionResult<ExchangeArmutCardsResult>.Failure(GameError.IllegalCard);

        var richPlayerState = state.Players.First(p => p.Id == command.RichPlayer);
        var poorPlayer = state.ArmutPlayer!.Value;
        var poorPlayerState = state.Players.First(p => p.Id == poorPlayer);

        // Validate all card IDs exist in the rich player's hand
        var richHand = richPlayerState.Hand;
        var cardsToReturn = command
            .CardIdsToReturn.Select(id => richHand.Cards.FirstOrDefault(c => c.Id == id))
            .ToList();

        if (cardsToReturn.Any(c => c is null))
            return new GameActionResult<ExchangeArmutCardsResult>.Failure(GameError.IllegalCard);

        var validCards = cardsToReturn.Select(c => c!).ToList();
        int returnedTrumpCount = validCards.Count(c => state.TrumpEvaluator.IsTrump(c.Type));

        // Move cards: remove from rich, add to poor
        var newRichHand = new Hand(richHand.Cards.Where(c => !validCards.Contains(c)).ToList());
        var newPoorHand = new Hand(poorPlayerState.Hand.Cards.Concat(validCards).ToList());

        state.Apply(new UpdatePlayerHandModification(command.RichPlayer, newRichHand));
        state.Apply(new UpdatePlayerHandModification(poorPlayer, newPoorHand));
        state.Apply(new SetArmutReturnedTrumpModification(returnedTrumpCount > 0));

        // The player seated immediately before the rich player in seat order leads the first trick.
        var richSeat = (int)state.Players.First(p => p.Id == command.RichPlayer).Seat;
        var precedingSeat = (richSeat - 1 + 4) % 4;
        var startingPlayer = state.Players.First(p => (int)p.Seat == precedingSeat).Id;

        state.Apply(new AdvancePhaseModification(GamePhase.Playing));
        state.Apply(new SetCurrentTurnModification(startingPlayer));

        var events = new List<IDomainEvent>
        {
            new ArmutCardsExchangedEvent(
                state.Id,
                command.RichPlayer,
                validCards.Count,
                returnedTrumpCount > 0
            ),
        };

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
        return new GameActionResult<ExchangeArmutCardsResult>.Ok(
            new ExchangeArmutCardsResult(returnedTrumpCount)
        );
    }
}
