using Doko.Application.Tests.Helpers;
using Doko.Domain.GameFlow;
using Doko.Domain.Parties;
using Doko.Domain.Trump;

namespace Doko.Application.Tests.Games.Handlers;

public class MakeAnnouncementHandlerTests
{
    private static async Task<(
        Fakes.InMemoryGameRepository,
        Fakes.RecordingGameEventPublisher,
        GameId
    )> PlayingGame()
    {
        var (repo, pub, _) = AppB.Infrastructure();

        // Solo resolver: P0 = Re, others = Kontra
        var players = new[]
        {
            new PlayerState(PlayerSeat.First, Hand.Empty, null),
            new PlayerState(PlayerSeat.Second, Hand.Empty, null),
            new PlayerState(PlayerSeat.Third, Hand.Empty, null),
            new PlayerState(PlayerSeat.Fourth, Hand.Empty, null),
        };

        var state = GameState.Create(
            phase: GamePhase.Playing,
            players: players,
            currentTurn: AppB.P0,
            partyResolver: new SoloPartyResolver(AppB.P0),
            rules: new RuleSet { AllowAnnouncements = true }
        );

        await repo.SaveAsync(state);
        return (repo, pub, state.Id);
    }

    [Fact]
    public async Task MakeAnnouncement_Re_Succeeds()
    {
        var (repo, pub, id) = await PlayingGame();
        var useCase = new MakeAnnouncementHandler(repo, pub);

        var result = await useCase.ExecuteAsync(
            new MakeAnnouncementCommand(id, AppB.P0, AnnouncementType.Win)
        );

        result.Should().BeOfType<GameActionResult<Unit>.Ok>();

        var state = (PlayingState)(await repo.GetAsync(id))!;
        state.Announcements.Should().HaveCount(1);
        state.Announcements[0].Type.Should().Be(AnnouncementType.Win);
    }

    [Fact]
    public async Task MakeAnnouncement_NotAllowed_ReturnsError()
    {
        var (repo, pub, id) = await PlayingGame();
        var useCase = new MakeAnnouncementHandler(repo, pub);

        // P0 is Re; Keine90 requires Re to be announced first
        var result = await useCase.ExecuteAsync(
            new MakeAnnouncementCommand(id, AppB.P0, AnnouncementType.Keine90)
        );

        result
            .Should()
            .BeOfType<GameActionResult<Unit>.Failure>()
            .Which.Error.Should()
            .Be(GameError.AnnouncementNotAllowed);
    }

    [Fact]
    public async Task MakeAnnouncement_GameNotFound_ReturnsError()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var useCase = new MakeAnnouncementHandler(repo, pub);

        var result = await useCase.ExecuteAsync(
            new MakeAnnouncementCommand(GameId.New(), AppB.P0, AnnouncementType.Win)
        );

        result
            .Should()
            .BeOfType<GameActionResult<Unit>.Failure>()
            .Which.Error.Should()
            .Be(GameError.GameNotFound);
    }

    [Fact]
    public async Task MakeAnnouncement_WrongPhase_ReturnsError()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var state = GameState.Create(
            phase: GamePhase.Dealing,
            players: [new PlayerState(PlayerSeat.First, Hand.Empty, null)]
        );
        await repo.SaveAsync(state);
        var useCase = new MakeAnnouncementHandler(repo, pub);

        var result = await useCase.ExecuteAsync(
            new MakeAnnouncementCommand(state.Id, AppB.P0, AnnouncementType.Win)
        );

        result
            .Should()
            .BeOfType<GameActionResult<Unit>.Failure>()
            .Which.Error.Should()
            .Be(GameError.InvalidPhase);
    }
}
