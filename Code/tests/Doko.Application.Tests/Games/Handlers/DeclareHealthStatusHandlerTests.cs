using Doko.Application.Tests.Helpers;

namespace Doko.Application.Tests.Games.Handlers;

public class DeclareHealthStatusHandlerTests
{
    private async Task<GameId> GameInHealthCheck(
        Fakes.InMemoryGameRepository repo,
        Fakes.RecordingGameEventPublisher pub
    )
    {
        var startResult = await new StartGameHandler(repo, pub).ExecuteAsync(
            new StartGameCommand(AppB.FourPlayerSeats, RuleSet.Minimal())
        );
        var id = ((GameActionResult<StartGameResult>.Ok)startResult).Value.GameId;
        await new DealCardsHandler(repo, pub, new Fakes.FakeDeckShuffler()).ExecuteAsync(
            new DealCardsCommand(id)
        );
        return id;
    }

    [Fact]
    public async Task DeclareHealth_WrongPhase_ReturnsInvalidPhase()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var startResult = await new StartGameHandler(repo, pub).ExecuteAsync(
            new StartGameCommand(AppB.FourPlayerSeats, RuleSet.Minimal())
        );
        var id = ((GameActionResult<StartGameResult>.Ok)startResult).Value.GameId;
        // Game is in Dealing phase, not ReservationHealthCheck
        var useCase = new DeclareHealthStatusHandler(repo, pub);

        var result = await useCase.ExecuteAsync(new DeclareHealthStatusCommand(id, AppB.P0, false));

        result
            .Should()
            .BeOfType<GameActionResult<DeclareHealthStatusResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.InvalidPhase);
    }

    [Fact]
    public async Task DeclareHealth_WrongPlayer_ReturnsNotYourTurn()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = await GameInHealthCheck(repo, pub);
        var useCase = new DeclareHealthStatusHandler(repo, pub);

        // P1 tries before P0 has gone
        var result = await useCase.ExecuteAsync(
            new DeclareHealthStatusCommand(gameId, AppB.P1, false)
        );

        result
            .Should()
            .BeOfType<GameActionResult<DeclareHealthStatusResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.NotYourTurn);
    }

    [Fact]
    public async Task DeclareHealth_NotLastPlayer_ReturnsFalseAllDeclared()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = await GameInHealthCheck(repo, pub);
        var useCase = new DeclareHealthStatusHandler(repo, pub);

        var result = await useCase.ExecuteAsync(
            new DeclareHealthStatusCommand(gameId, AppB.P0, false)
        );

        result
            .Should()
            .BeOfType<GameActionResult<DeclareHealthStatusResult>.Ok>()
            .Which.Value.AllDeclared.Should()
            .BeFalse();
    }

    [Fact]
    public async Task DeclareHealth_AllVorbehalt_AdvancesToSoloCheckWithAllFourPlayers()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = await GameInHealthCheck(repo, pub);
        var useCase = new DeclareHealthStatusHandler(repo, pub);

        foreach (var player in AppB.FourPlayerSeats)
            await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, player, true));

        var state = await repo.GetAsync(gameId);
        state!.Phase.Should().Be(GamePhase.ReservationSoloCheck);
        state.PendingReservationResponders.Should().BeEquivalentTo(AppB.FourPlayerSeats);
    }

    [Fact]
    public async Task DeclareHealth_SingleVorbehalt_AdvancesToSoloCheckWithOnlyThatPlayer()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = await GameInHealthCheck(repo, pub);
        var useCase = new DeclareHealthStatusHandler(repo, pub);

        // Only P2 has a Vorbehalt
        await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, AppB.P0, false));
        await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, AppB.P1, false));
        await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, AppB.P2, true));
        await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, AppB.P3, false));

        var state = await repo.GetAsync(gameId);
        state!.Phase.Should().Be(GamePhase.ReservationSoloCheck);
        state.PendingReservationResponders.Should().ContainSingle().Which.Should().Be(AppB.P2);
    }

    [Fact]
    public async Task DeclareHealth_LastPlayer_ReturnsTrueAllDeclared()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = await GameInHealthCheck(repo, pub);
        var useCase = new DeclareHealthStatusHandler(repo, pub);

        GameActionResult<DeclareHealthStatusResult> lastResult = null!;
        foreach (var player in AppB.FourPlayerSeats)
            lastResult = await useCase.ExecuteAsync(
                new DeclareHealthStatusCommand(gameId, player, false)
            );

        lastResult
            .Should()
            .BeOfType<GameActionResult<DeclareHealthStatusResult>.Ok>()
            .Which.Value.AllDeclared.Should()
            .BeTrue();
    }

    [Fact]
    public async Task DeclareHealth_NoVorbehalt_SetsCurrentTurnToVorbehaltRauskommer()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var startResult = await new StartGameHandler(repo, pub).ExecuteAsync(
            new StartGameCommand(AppB.FourPlayerSeats, RuleSet.Minimal())
        );
        var gameId = ((GameActionResult<StartGameResult>.Ok)startResult).Value.GameId;
        // P2 is the VorbehaltRauskommer — health check starts from P2, P3, P0, P1
        await new DealCardsHandler(repo, pub, new Fakes.FakeDeckShuffler()).ExecuteAsync(
            new DealCardsCommand(gameId, AppB.P2)
        );
        var useCase = new DeclareHealthStatusHandler(repo, pub);

        await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, AppB.P2, false));
        await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, AppB.P3, false));
        await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, AppB.P0, false));
        await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, AppB.P1, false));

        var state = await repo.GetAsync(gameId);
        state!.Phase.Should().Be(GamePhase.Playing);
        state.CurrentTurn.Should().Be(AppB.P2);
    }

    [Fact]
    public async Task DeclareHealth_VorbehaltPlayers_OrderedFromVorbehaltRauskommer()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var startResult = await new StartGameHandler(repo, pub).ExecuteAsync(
            new StartGameCommand(AppB.FourPlayerSeats, RuleSet.Minimal())
        );
        var gameId = ((GameActionResult<StartGameResult>.Ok)startResult).Value.GameId;
        // VorbehaltRauskommer = P2, health check order: P2, P3, P0, P1
        // P3 and P1 have Vorbehalt; relative to P2: P3 (rank 1) before P1 (rank 3)
        await new DealCardsHandler(repo, pub, new Fakes.FakeDeckShuffler()).ExecuteAsync(
            new DealCardsCommand(gameId, AppB.P2)
        );
        var useCase = new DeclareHealthStatusHandler(repo, pub);

        await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, AppB.P2, false));
        await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, AppB.P3, true));
        await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, AppB.P0, false));
        await useCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, AppB.P1, true));

        var state = await repo.GetAsync(gameId);
        state!.Phase.Should().Be(GamePhase.ReservationSoloCheck);
        state.PendingReservationResponders.Should().ContainInOrder(AppB.P3, AppB.P1);
        state.CurrentTurn.Should().Be(AppB.P3);
    }
}
