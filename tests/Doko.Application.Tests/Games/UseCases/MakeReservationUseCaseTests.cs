using Doko.Application.Tests.Helpers;

namespace Doko.Application.Tests.Games.UseCases;

public class MakeReservationUseCaseTests
{
    /// <summary>
    /// Creates a game, deals cards, and completes the health check with all players saying Gesund.
    /// Returns the gameId; after this the state is in ReservationSoloCheck.
    /// </summary>
    private async Task<GameId> GameInSoloCheckPhase(
        Doko.Application.Tests.Fakes.InMemoryGameRepository repo,
        Doko.Application.Tests.Fakes.RecordingGameEventPublisher pub
    )
    {
        var id = (
            (GameActionResult<StartGameResult>.Ok)
                await new StartGameUseCase(repo, pub).ExecuteAsync(
                    new StartGameCommand(AppB.FourPlayerIds, RuleSet.Minimal())
                )
        )
            .Value
            .GameId;
        await new DealCardsUseCase(repo, pub, new Fakes.FakeDeckShuffler()).ExecuteAsync(
            new DealCardsCommand(id)
        );
        // All players say Gesund to skip health check
        var healthUseCase = new DeclareHealthStatusUseCase(repo, pub);
        foreach (var player in AppB.FourPlayerIds)
            await healthUseCase.ExecuteAsync(new DeclareHealthStatusCommand(id, player, false));
        return id;
    }

    /// <summary>
    /// Overload where all players say Vorbehalt to get into SoloCheck with multiple Vorbehalt.
    /// </summary>
    private async Task<GameId> GameInSoloCheckPhaseAllVorbehalt(
        Doko.Application.Tests.Fakes.InMemoryGameRepository repo,
        Doko.Application.Tests.Fakes.RecordingGameEventPublisher pub
    )
    {
        var id = (
            (GameActionResult<StartGameResult>.Ok)
                await new StartGameUseCase(repo, pub).ExecuteAsync(
                    new StartGameCommand(AppB.FourPlayerIds, RuleSet.Minimal())
                )
        )
            .Value
            .GameId;
        await new DealCardsUseCase(repo, pub, new Fakes.FakeDeckShuffler()).ExecuteAsync(
            new DealCardsCommand(id)
        );
        var healthUseCase = new DeclareHealthStatusUseCase(repo, pub);
        foreach (var player in AppB.FourPlayerIds)
            await healthUseCase.ExecuteAsync(new DeclareHealthStatusCommand(id, player, true));
        return id;
    }

    [Fact]
    public async Task MakeReservation_ReturnsNotAllDeclared_WhenThreePlayersLeft()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = await GameInSoloCheckPhaseAllVorbehalt(repo, pub);
        var useCase = new MakeReservationUseCase(repo, pub);

        // P0 passes (no solo)
        var result = await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P0, null));

        result
            .Should()
            .BeOfType<GameActionResult<MakeReservationResult>.Ok>()
            .Which.Value.AllDeclared.Should()
            .BeFalse();
    }

    [Fact]
    public async Task MakeReservation_AllNormal_AdvancesToArmutCheck()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = await GameInSoloCheckPhaseAllVorbehalt(repo, pub);
        var useCase = new MakeReservationUseCase(repo, pub);

        // All four Vorbehalt players pass on Solo
        await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P0, null));
        await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P1, null));
        await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P2, null));
        var result = await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P3, null));

        result.Should().BeOfType<GameActionResult<MakeReservationResult>.Ok>();
        var ok = (GameActionResult<MakeReservationResult>.Ok)result;
        ok.Value.AllDeclared.Should().BeFalse();

        var state = await repo.GetAsync(gameId);
        state!.Phase.Should().Be(GamePhase.ReservationArmutCheck);
    }

    [Fact]
    public async Task MakeReservation_AlreadyDeclared_ReturnsNotYourTurn()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = await GameInSoloCheckPhaseAllVorbehalt(repo, pub);
        var useCase = new MakeReservationUseCase(repo, pub);

        // P0 declares; now it's P1's turn
        await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P0, null));
        // P0 tries again — no longer in the pending queue
        var result = await useCase.ExecuteAsync(new MakeReservationCommand(gameId, AppB.P0, null));

        result
            .Should()
            .BeOfType<GameActionResult<MakeReservationResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.NotYourTurn);
    }

    [Fact]
    public async Task MakeReservation_WrongPhase_ReturnsInvalidPhase()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        // Game in Dealing phase (not yet dealt)
        var id = (
            (GameActionResult<StartGameResult>.Ok)
                await new StartGameUseCase(repo, pub).ExecuteAsync(
                    new StartGameCommand(AppB.FourPlayerIds)
                )
        )
            .Value
            .GameId;
        var useCase = new MakeReservationUseCase(repo, pub);

        var result = await useCase.ExecuteAsync(new MakeReservationCommand(id, AppB.P0, null));

        result
            .Should()
            .BeOfType<GameActionResult<MakeReservationResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.InvalidPhase);
    }

    [Fact]
    public async Task MakeReservation_AllPassNoSolos_AllArmutPass_AllSchmeissenPass_AllHochzeitPass_AdvancesToPlaying()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = await GameInSoloCheckPhaseAllVorbehalt(repo, pub);
        var useCase = new MakeReservationUseCase(repo, pub);

        // Pass all four check phases (minimal rules: no solos, no armut, no schmeissen, no hochzeit)
        // SoloCheck
        foreach (var player in AppB.FourPlayerIds)
            await useCase.ExecuteAsync(new MakeReservationCommand(gameId, player, null));

        // ArmutCheck
        foreach (var player in AppB.FourPlayerIds)
            await useCase.ExecuteAsync(new MakeReservationCommand(gameId, player, null));

        // SchmeissenCheck
        foreach (var player in AppB.FourPlayerIds)
            await useCase.ExecuteAsync(new MakeReservationCommand(gameId, player, null));

        // HochzeitCheck — last player in Hochzeit check will result in forced Schlanker Martin or normal game
        foreach (var player in AppB.FourPlayerIds)
            await useCase.ExecuteAsync(new MakeReservationCommand(gameId, player, null));

        var state = await repo.GetAsync(gameId);
        // With minimal rules (no Schlanker Martin), should end up in Playing
        state!.Phase.Should().Be(GamePhase.Playing);
    }

    [Fact]
    public async Task DeclareHealth_AllGesund_AdvancesToPlaying()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var gameId = ((GameActionResult<StartGameResult>.Ok)
                await new StartGameUseCase(repo, pub).ExecuteAsync(
                    new StartGameCommand(AppB.FourPlayerIds, RuleSet.Minimal())
                ))
            .Value
            .GameId;
        await new DealCardsUseCase(repo, pub, new Fakes.FakeDeckShuffler()).ExecuteAsync(
            new DealCardsCommand(gameId)
        );

        var healthUseCase = new DeclareHealthStatusUseCase(repo, pub);
        foreach (var player in AppB.FourPlayerIds)
            await healthUseCase.ExecuteAsync(new DeclareHealthStatusCommand(gameId, player, false));

        var state = await repo.GetAsync(gameId);
        // All Gesund → skip all checks → directly to Playing
        state!.Phase.Should().Be(GamePhase.Playing);
    }
}
