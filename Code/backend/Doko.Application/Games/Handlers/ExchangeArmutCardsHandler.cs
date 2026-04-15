using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Hands;
using Doko.Domain.Players;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.Handlers;

public interface IExchangeArmutCardsHandler
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
public sealed class ExchangeArmutCardsHandler(
    IGameRepository repository,
    IGameEventPublisher publisher
) : IExchangeArmutCardsHandler
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

        var validCardsResult = ResolveCardsToReturn(state, command);
        if (validCardsResult is null)
            return new GameActionResult<ExchangeArmutCardsResult>.Failure(GameError.IllegalCard);

        var poorPlayer = state.ArmutPlayer!.Value;
        var (newRichHand, newPoorHand, returnedTrumpCount) = ComputeNewHands(
            state,
            command,
            poorPlayer,
            validCardsResult
        );

        ApplyHandUpdates(
            state,
            command.RichPlayer,
            poorPlayer,
            newRichHand,
            newPoorHand,
            returnedTrumpCount
        );

        var startingPlayer = FindStartingPlayer(state, command.RichPlayer, poorPlayer);
        state.Apply(new AdvancePhaseModification(GamePhase.Playing));
        state.Apply(new SetCurrentTurnModification(startingPlayer));

        var events = new List<IDomainEvent>
        {
            new ArmutCardsExchangedEvent(
                state.Id,
                command.RichPlayer,
                validCardsResult.Count,
                returnedTrumpCount > 0
            ),
        };

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, events, ct);
        return new GameActionResult<ExchangeArmutCardsResult>.Ok(
            new ExchangeArmutCardsResult(returnedTrumpCount)
        );
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the card objects for the given IDs from the rich player's hand.
    /// Returns null if any ID is not found.
    /// </summary>
    private static List<Card>? ResolveCardsToReturn(
        GameState state,
        ExchangeArmutCardsCommand command
    )
    {
        var richHand = state.Players.First(p => p.Id == command.RichPlayer).Hand;
        var cards = command
            .CardIdsToReturn.Select(id => richHand.Cards.FirstOrDefault(c => c.Id == id))
            .ToList();

        if (cards.Any(c => c is null))
            return null;

        return cards.Select(c => c!).ToList();
    }

    /// <summary>
    /// Computes the new hands for both players and counts returned trumps.
    /// </summary>
    private static (Hand newRichHand, Hand newPoorHand, int returnedTrumpCount) ComputeNewHands(
        GameState state,
        ExchangeArmutCardsCommand command,
        PlayerId poorPlayer,
        List<Card> validCards
    )
    {
        var richHand = state.Players.First(p => p.Id == command.RichPlayer).Hand;
        var poorHand = state.Players.First(p => p.Id == poorPlayer).Hand;

        int returnedTrumpCount = validCards.Count(c => state.TrumpEvaluator.IsTrump(c.Type));
        var newRichHand = new Hand(richHand.Cards.Where(c => !validCards.Contains(c)).ToList());
        var newPoorHand = new Hand(poorHand.Cards.Concat(validCards).ToList());

        return (newRichHand, newPoorHand, returnedTrumpCount);
    }

    private static void ApplyHandUpdates(
        GameState state,
        PlayerId richPlayer,
        PlayerId poorPlayer,
        Hand newRichHand,
        Hand newPoorHand,
        int returnedTrumpCount
    )
    {
        state.Apply(new UpdatePlayerHandModification(richPlayer, newRichHand));
        state.Apply(new UpdatePlayerHandModification(poorPlayer, newPoorHand));
        state.Apply(new SetArmutReturnedTrumpModification(returnedTrumpCount > 0));
    }

    /// <summary>
    /// Finds the first player to the left of the rich player who is not in the rich party.
    /// </summary>
    private static PlayerId FindStartingPlayer(
        GameState state,
        PlayerId richPlayer,
        PlayerId poorPlayer
    )
    {
        var richSeat = (int)state.Players.First(p => p.Id == richPlayer).Seat;
        return state
            .Players.OrderBy(p => ((int)p.Seat - richSeat - 1 + 4) % 4)
            .First(p => p.Id != richPlayer && p.Id != poorPlayer)
            .Id;
    }
}
