using Doko.Application.Tests.Helpers;

namespace Doko.Application.Tests.Games.UseCases;

public class DealCardsUseCaseTests
{
    private async Task<GameId> StartedGame(
        Doko.Application.Tests.Fakes.InMemoryGameRepository repo,
        Doko.Application.Tests.Fakes.RecordingGameEventPublisher pub
    )
    {
        var startResult = await new StartGameUseCase(repo, pub).ExecuteAsync(
            new StartGameCommand(AppB.FourPlayerIds, RuleSet.Minimal())
        );
        return ((GameActionResult<StartGameResult>.Ok)startResult).Value.GameId;
    }

    [Fact]
    public async Task DealCards_DealsHandsToAllPlayers()
    {
        var (repo, pub, shuffler) = AppB.Infrastructure();
        var gameId = await StartedGame(repo, pub);
        var useCase = new DealCardsUseCase(repo, pub, shuffler);

        var result = await useCase.ExecuteAsync(new DealCardsCommand(gameId));

        result.Should().BeOfType<GameActionResult<Unit>.Ok>();

        var state = await repo.GetAsync(gameId);
        state!.Phase.Should().Be(GamePhase.Reservations);
        state.Players.Should().AllSatisfy(p => p.Hand.Cards.Should().NotBeEmpty());
    }

    [Fact]
    public async Task DealCards_GivesEachPlayerEqualCards_WithNoNines()
    {
        var (repo, pub, shuffler) = AppB.Infrastructure();
        var gameId = await StartedGame(repo, pub);
        var useCase = new DealCardsUseCase(repo, pub, shuffler);

        await useCase.ExecuteAsync(new DealCardsCommand(gameId));

        var state = await repo.GetAsync(gameId);
        // Minimal rules: PlayWithNines = false → 40-card deck / 4 = 10 cards each
        state!.Players.Should().AllSatisfy(p => p.Hand.Cards.Count.Should().Be(10));
    }

    [Fact]
    public async Task DealCards_ReturnsGameNotFound_WhenGameDoesNotExist()
    {
        var (repo, pub, shuffler) = AppB.Infrastructure();
        var useCase = new DealCardsUseCase(repo, pub, shuffler);

        var result = await useCase.ExecuteAsync(new DealCardsCommand(GameId.New()));

        result
            .Should()
            .BeOfType<GameActionResult<Unit>.Failure>()
            .Which.Error.Should()
            .Be(GameError.GameNotFound);
    }

    [Fact]
    public async Task DealCards_ReturnsInvalidPhase_WhenNotInDealingPhase()
    {
        var (repo, pub, shuffler) = AppB.Infrastructure();
        var gameId = await StartedGame(repo, pub);
        // Deal once to move to Reservations
        await new DealCardsUseCase(repo, pub, shuffler).ExecuteAsync(new DealCardsCommand(gameId));

        var result = await new DealCardsUseCase(repo, pub, shuffler).ExecuteAsync(
            new DealCardsCommand(gameId)
        );

        result
            .Should()
            .BeOfType<GameActionResult<Unit>.Failure>()
            .Which.Error.Should()
            .Be(GameError.InvalidPhase);
    }
}
