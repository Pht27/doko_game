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
using static Doko.Application.Common.GameActionResultExtensions;

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
        var loaded = await repository.LoadOrFailAsync<ExchangeArmutCardsResult>(command.GameId, ct);
        if (loaded.Failure is not null)
            return loaded.Failure;
        var state = loaded.State!;

        if (state.Phase != GamePhase.ArmutCardExchange)
            return Fail<ExchangeArmutCardsResult>(GameError.InvalidPhase);

        if (state.ArmutRichPlayer != command.RichPlayer)
            return Fail<ExchangeArmutCardsResult>(GameError.NotYourTurn);

        if (command.CardIdsToReturn.Count != state.ArmutTransferCount)
            return Fail<ExchangeArmutCardsResult>(GameError.IllegalCard);

        var validCardsResult = ResolveCardsToReturn(state, command);
        if (validCardsResult is null)
            return Fail<ExchangeArmutCardsResult>(GameError.IllegalCard);

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
        return Ok(new ExchangeArmutCardsResult(returnedTrumpCount));
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
        var richHand = state.Players.First(p => p.Seat == command.RichPlayer).Hand;
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
        PlayerSeat poorPlayer,
        List<Card> validCards
    )
    {
        var richHand = state.Players.First(p => p.Seat == command.RichPlayer).Hand;
        var poorHand = state.Players.First(p => p.Seat == poorPlayer).Hand;

        int returnedTrumpCount = validCards.Count(c => state.TrumpEvaluator.IsTrump(c.Type));
        var newRichHand = new Hand(richHand.Cards.Where(c => !validCards.Contains(c)).ToList());
        var newPoorHand = new Hand(poorHand.Cards.Concat(validCards).ToList());

        return (newRichHand, newPoorHand, returnedTrumpCount);
    }

    private static void ApplyHandUpdates(
        GameState state,
        PlayerSeat richPlayer,
        PlayerSeat poorPlayer,
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
    /// Finds the first player to the left of the rich player who is not in the Re party
    /// (neither rich nor poor). This player leads the first trick after the Armut exchange.
    /// </summary>
    private static PlayerSeat FindStartingPlayer(
        GameState state,
        PlayerSeat richPlayer,
        PlayerSeat poorPlayer
    )
    {
        var richSeat = (int)richPlayer;
        return state
            .Players.OrderBy(p => ((int)p.Seat - richSeat - 1 + 4) % 4)
            .First(p => p.Seat != richPlayer && p.Seat != poorPlayer)
            .Seat;
    }
}
