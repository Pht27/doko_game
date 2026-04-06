using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Hands;
using Doko.Domain.Players;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.UseCases;

public interface IDealCardsUseCase
{
    Task<GameActionResult<Unit>> ExecuteAsync(
        DealCardsCommand command,
        CancellationToken ct = default
    );
}

public sealed class DealCardsUseCase(
    IGameRepository repository,
    IGameEventPublisher publisher,
    IDeckShuffler shuffler
) : IDealCardsUseCase
{
    public async Task<GameActionResult<Unit>> ExecuteAsync(
        DealCardsCommand command,
        CancellationToken ct = default
    )
    {
        var state = await repository.GetAsync(command.GameId, ct);
        if (state is null)
            return new GameActionResult<Unit>.Failure(GameError.GameNotFound);

        if (state.Phase != GamePhase.Dealing)
            return new GameActionResult<Unit>.Failure(GameError.InvalidPhase);

        var deck = state.Rules.PlayWithNines ? Deck.Standard48() : Deck.Standard40();
        var shuffled = shuffler.Shuffle(deck);
        var cardsPerPlayer = shuffled.Count / 4;

        var hands = new Dictionary<PlayerId, Hand>();
        for (int i = 0; i < state.Players.Count; i++)
        {
            var playerCards = shuffled.Skip(i * cardsPerPlayer).Take(cardsPerPlayer).ToList();
            hands[state.Players[i].Id] = new Hand(playerCards);
        }

        state.Apply(new DealHandsModification(hands));
        state.Apply(new AdvancePhaseModification(GamePhase.Reservations));
        state.Apply(new SetCurrentTurnModification(state.Players[0].Id));

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, [], ct);

        return new GameActionResult<Unit>.Ok(Unit.Value);
    }
}
