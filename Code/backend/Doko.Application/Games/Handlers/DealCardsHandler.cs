using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Scenarios;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Modifications;
using Doko.Domain.Hands;
using Doko.Domain.Players;
using static Doko.Application.Common.GameActionResultExtensions;

namespace Doko.Application.Games.Handlers;

public interface IDealCardsHandler
{
    Task<GameActionResult<Unit>> ExecuteAsync(
        DealCardsCommand command,
        CancellationToken ct = default
    );
}

public sealed class DealCardsHandler(
    IGameRepository repository,
    IGameEventPublisher publisher,
    IDeckShuffler shuffler,
    IScenarioShufflerFactory scenarioShufflerFactory
) : IDealCardsHandler
{
    public Task<GameActionResult<Unit>> ExecuteAsync(
        DealCardsCommand command,
        CancellationToken ct = default
    ) =>
        GameCommandPipeline.RunAsync<Unit>(
            repository,
            publisher,
            command.GameId,
            GamePhase.Dealing,
            execute: state =>
            {
                var deck = state.Rules.PlayWithNines ? Deck.Standard48() : Deck.Standard40();
                var activeShuffler =
                    scenarioShufflerFactory.TryCreate(command.ScenarioName) ?? shuffler;
                var shuffled = activeShuffler.Shuffle(deck);
                var cardsPerPlayer = shuffled.Count / 4;

                var hands = new Dictionary<PlayerSeat, Hand>();
                for (int i = 0; i < state.Players.Count; i++)
                {
                    var playerCards = shuffled
                        .Skip(i * cardsPerPlayer)
                        .Take(cardsPerPlayer)
                        .ToList();
                    hands[state.Players[i].Seat] = new Hand(playerCards);
                }

                state = state.Apply(new DealHandsModification(hands));
                state = state.Apply(new AdvancePhaseModification(GamePhase.ReservationHealthCheck));

                var rauskommer = command.VorbehaltRauskommer ?? state.Players[0].Seat;
                state = state.Apply(new SetVorbehaltRauskommerModification(rauskommer));
                var rauskommerSeat = (int)rauskommer;
                var allPlayers = state
                    .Players.OrderBy(p => ((int)p.Seat - rauskommerSeat + 4) % 4)
                    .Select(p => p.Seat)
                    .ToList();
                state = state.Apply(new SetPendingRespondersModification(allPlayers));
                state = state.Apply(new SetCurrentTurnModification(rauskommer));

                return (Ok(Unit.Value), [], state);
            },
            ct
        );
}
