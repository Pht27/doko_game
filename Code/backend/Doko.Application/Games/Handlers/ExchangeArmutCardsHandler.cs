using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.GameFlow.Modifications;
using Doko.Domain.Hands;
using Doko.Domain.Players;
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
    public Task<GameActionResult<ExchangeArmutCardsResult>> ExecuteAsync(
        ExchangeArmutCardsCommand command,
        CancellationToken ct = default
    ) =>
        GameCommandPipeline.RunAsync<ExchangeArmutCardsResult, ArmutFlowState>(
            repository,
            publisher,
            command.GameId,
            execute: (ArmutFlowState state) =>
            {
                if (state.Phase != GamePhase.ArmutCardExchange)
                    return (Fail<ExchangeArmutCardsResult>(GameError.InvalidPhase), [], state);

                if (state.Armut?.RichPlayer != command.RichPlayer)
                    return (Fail<ExchangeArmutCardsResult>(GameError.NotYourTurn), [], state);

                if (command.CardIdsToReturn.Count != state.Armut!.TransferCount)
                    return (Fail<ExchangeArmutCardsResult>(GameError.IllegalCard), [], state);

                var cardsToReturn = ResolveCardsToReturn(state, command);
                if (cardsToReturn is null)
                    return (Fail<ExchangeArmutCardsResult>(GameError.IllegalCard), [], state);

                var poorPlayer = state.Armut.Player;
                var (newRichHand, newPoorHand, returnedTrumpCount) = ComputeNewHands(
                    (GameState)state,
                    command,
                    poorPlayer,
                    cardsToReturn
                );

                GameState nextState = state;
                nextState = nextState.Apply(
                    new UpdatePlayerHandModification(command.RichPlayer, newRichHand)
                );
                nextState = nextState.Apply(new UpdatePlayerHandModification(poorPlayer, newPoorHand));
                nextState = nextState.Apply(new SetArmutReturnedTrumpModification(returnedTrumpCount > 0));

                var startingPlayer = FindStartingPlayer(nextState, command.RichPlayer, poorPlayer);
                nextState = nextState.Apply(new AdvancePhaseModification(GamePhase.Playing));
                nextState = nextState.Apply(new SetCurrentTurnModification(startingPlayer));

                return (
                    Ok(new ExchangeArmutCardsResult(returnedTrumpCount)),
                    [
                        new ArmutCardsExchangedEvent(
                            nextState.Id,
                            command.RichPlayer,
                            cardsToReturn.Count,
                            returnedTrumpCount > 0
                        ),
                    ],
                    nextState
                );
            },
            ct
        );

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
