using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Scenarios;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Hands;
using Doko.Domain.Players;
using Doko.Domain.Sonderkarten;
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
    IDeckShuffler shuffler
) : IDealCardsHandler
{
    public async Task<GameActionResult<Unit>> ExecuteAsync(
        DealCardsCommand command,
        CancellationToken ct = default
    )
    {
        var loaded = await repository.LoadOrFailAsync<Unit>(command.GameId, ct);
        if (loaded.Failure is not null)
            return loaded.Failure;
        var state = loaded.State!;

        if (state.Phase != GamePhase.Dealing)
            return Fail<Unit>(GameError.InvalidPhase);

        var deck = state.Rules.PlayWithNines ? Deck.Standard48() : Deck.Standard40();
        var activeShuffler = ResolveShuffler(command.ScenarioName);
        var shuffled = activeShuffler.Shuffle(deck);
        var cardsPerPlayer = shuffled.Count / 4;

        var hands = new Dictionary<PlayerSeat, Hand>();
        for (int i = 0; i < state.Players.Count; i++)
        {
            var playerCards = shuffled.Skip(i * cardsPerPlayer).Take(cardsPerPlayer).ToList();
            hands[state.Players[i].Seat] = new Hand(playerCards);
        }

        state.Apply(new DealHandsModification(hands));
        state.Apply(new AdvancePhaseModification(GamePhase.ReservationHealthCheck));

        var rauskommer = command.VorbehaltRauskommer ?? state.Players[0].Seat;
        state.Apply(new SetVorbehaltRauskommerModification(rauskommer));
        var rauskommerSeat = (int)rauskommer;
        var allPlayers = state
            .Players.OrderBy(p => ((int)p.Seat - rauskommerSeat + 4) % 4)
            .Select(p => p.Seat)
            .ToList();
        state.Apply(new SetPendingRespondersModification(allPlayers));
        state.Apply(new SetCurrentTurnModification(rauskommer));

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id, [], ct);

        return Ok(Unit.Value);
    }

    private IDeckShuffler ResolveShuffler(string? scenarioName)
    {
        if (scenarioName is null)
            return shuffler;
        var config = Scenarios.Scenarios.All.FirstOrDefault(s => s.Name == scenarioName);
        return config is not null ? new ScenarioShuffler(config) : shuffler;
    }
}
