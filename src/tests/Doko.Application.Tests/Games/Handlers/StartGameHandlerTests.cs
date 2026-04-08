using Doko.Application.Tests.Helpers;

namespace Doko.Application.Tests.Games.Handlers;

public class StartGameHandlerTests
{
    [Fact]
    public async Task StartGame_CreatesGameInDealingPhase()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var useCase = new StartGameHandler(repo, pub);

        var result = await useCase.ExecuteAsync(new StartGameCommand(AppB.FourPlayerIds));

        result.Should().BeOfType<GameActionResult<StartGameResult>.Ok>();
        var ok = (GameActionResult<StartGameResult>.Ok)result;

        var saved = await repo.GetAsync(ok.Value.GameId);
        saved.Should().NotBeNull();
        saved!.Phase.Should().Be(GamePhase.Dealing);
        saved.Players.Should().HaveCount(4);
    }

    [Fact]
    public async Task StartGame_AssignsSeatsInOrder()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var useCase = new StartGameHandler(repo, pub);

        var result = await useCase.ExecuteAsync(new StartGameCommand(AppB.FourPlayerIds));
        var ok = (GameActionResult<StartGameResult>.Ok)result;
        var saved = await repo.GetAsync(ok.Value.GameId);

        saved!
            .Players.Select(p => p.Seat)
            .Should()
            .Equal(PlayerSeat.First, PlayerSeat.Second, PlayerSeat.Third, PlayerSeat.Fourth);
    }

    [Fact]
    public async Task StartGame_WithWrongPlayerCount_ReturnsInvalidPhase()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var useCase = new StartGameHandler(repo, pub);

        var result = await useCase.ExecuteAsync(new StartGameCommand([AppB.P0, AppB.P1]));

        result
            .Should()
            .BeOfType<GameActionResult<StartGameResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.InvalidPhase);
    }
}
