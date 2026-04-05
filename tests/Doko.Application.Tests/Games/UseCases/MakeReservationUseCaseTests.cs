using Doko.Application.Tests.Helpers;

namespace Doko.Application.Tests.Games.UseCases;

public class MakeReservationUseCaseTests
{
    private async Task<GameId> DealtGame(
        Doko.Application.Tests.Fakes.InMemoryGameRepository repo,
        Doko.Application.Tests.Fakes.RecordingGameEventPublisher pub)
    {
        var id = ((GameActionResult<StartGameResult>.Ok)
            await new StartGameUseCase(repo, pub)
                .ExecuteAsync(new StartGameCommand(AppB.FourPlayerIds, RuleSet.Minimal()))).Value.GameId;
        await new DealCardsUseCase(repo, pub, new Fakes.FakeDeckShuffler()).ExecuteAsync(new DealCardsCommand(id));
        return id;
    }

    [Fact]
    public async Task MakeReservation_ReturnsNotAllDeclared_WhenThreePlayersLeft()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = await DealtGame(repo, pub);
        var useCase = new MakeReservationUseCase(repo, pub);

        var result = await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P0, null));

        result.Should().BeOfType<GameActionResult<MakeReservationResult>.Ok>()
            .Which.Value.AllDeclared.Should().BeFalse();
    }

    [Fact]
    public async Task MakeReservation_AllNormal_AdvancesToPlaying()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = await DealtGame(repo, pub);
        var useCase = new MakeReservationUseCase(repo, pub);

        await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P0, null));
        await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P1, null));
        await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P2, null));
        var result = await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P3, null));

        result.Should().BeOfType<GameActionResult<MakeReservationResult>.Ok>();
        var ok = (GameActionResult<MakeReservationResult>.Ok)result;
        ok.Value.AllDeclared.Should().BeTrue();
        ok.Value.WinningReservation.Should().BeNull();

        var state = await repo.GetAsync(gameId);
        state!.Phase.Should().Be(GamePhase.Playing);
    }

    [Fact]
    public async Task MakeReservation_AlreadyDeclared_ReturnsError()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = await DealtGame(repo, pub);
        var useCase = new MakeReservationUseCase(repo, pub);

        await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P0, null));
        var result = await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P0, null));

        result.Should().BeOfType<GameActionResult<MakeReservationResult>.Failure>()
            .Which.Error.Should().Be(GameError.AlreadyDeclared);
    }

    [Fact]
    public async Task MakeReservation_WrongPhase_ReturnsInvalidPhase()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        // Game in Dealing phase (not yet dealt)
        var id = ((GameActionResult<StartGameResult>.Ok)
            await new StartGameUseCase(repo, pub)
                .ExecuteAsync(new StartGameCommand(AppB.FourPlayerIds))).Value.GameId;
        var useCase = new MakeReservationUseCase(repo, pub);

        var result = await useCase.ExecuteAsync(new MakeReservationCommand(id, AppB.P0, null));

        result.Should().BeOfType<GameActionResult<MakeReservationResult>.Failure>()
            .Which.Error.Should().Be(GameError.InvalidPhase);
    }
}
