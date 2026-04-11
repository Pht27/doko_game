using Doko.Application.Tests.Helpers;

namespace Doko.Application.Tests.Games.Handlers;

public class AcceptArmutHandlerTests
{
    // Cards used across tests — IDs are arbitrary but unique
    private static readonly Card Trump1 = AppB.Card(0, Suit.Kreuz, Rank.Bube);
    private static readonly Card Trump2 = AppB.Card(1, Suit.Kreuz, Rank.Dame);
    private static readonly Card NonTrump0 = AppB.Card(2, Suit.Kreuz, Rank.Koenig);
    private static readonly Card NonTrump1 = AppB.Card(3, Suit.Pik, Rank.Koenig);
    private static readonly Card NonTrump2 = AppB.Card(4, Suit.Pik, Rank.Ass);
    private static readonly Card NonTrump3 = AppB.Card(5, Suit.Herz, Rank.Koenig);

    /// <summary>
    /// Builds a game in <see cref="GamePhase.ArmutPartnerFinding"/>.
    /// P0 is the poor player with 2 trumps; P1–P3 are partner candidates.
    /// </summary>
    private static async Task<(
        Fakes.InMemoryGameRepository repo,
        Fakes.RecordingGameEventPublisher pub,
        GameId id
    )> ArmutPartnerFindingGame()
    {
        var (repo, pub, _) = AppB.Infrastructure();

        var players = new[]
        {
            new PlayerState(
                AppB.P0,
                PlayerSeat.First,
                AppB.HandOf(Trump1, Trump2, NonTrump0),
                null
            ),
            new PlayerState(AppB.P1, PlayerSeat.Second, AppB.HandOf(NonTrump1), null),
            new PlayerState(AppB.P2, PlayerSeat.Third, AppB.HandOf(NonTrump2), null),
            new PlayerState(AppB.P3, PlayerSeat.Fourth, AppB.HandOf(NonTrump3), null),
        };

        var state = GameState.Create(
            phase: GamePhase.ArmutPartnerFinding,
            players: players,
            currentTurn: AppB.P1
        );
        state.Apply(new SetArmutPlayerModification(AppB.P0));
        state.Apply(new SetPendingRespondersModification([AppB.P1, AppB.P2, AppB.P3]));

        await repo.SaveAsync(state);
        return (repo, pub, state.Id);
    }

    [Fact]
    public async Task AcceptArmut_WrongPhase_ReturnsInvalidPhase()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var state = GameState.Create(phase: GamePhase.Playing, currentTurn: AppB.P1);
        await repo.SaveAsync(state);
        var useCase = new AcceptArmutHandler(repo, pub);

        var result = await useCase.ExecuteAsync(new AcceptArmutCommand(state.Id, AppB.P1, true));

        result
            .Should()
            .BeOfType<GameActionResult<AcceptArmutResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.InvalidPhase);
    }

    [Fact]
    public async Task AcceptArmut_WrongPlayer_ReturnsNotYourTurn()
    {
        var (repo, pub, id) = await ArmutPartnerFindingGame();
        var useCase = new AcceptArmutHandler(repo, pub);

        // P2 tries when it's P1's turn
        var result = await useCase.ExecuteAsync(new AcceptArmutCommand(id, AppB.P2, true));

        result
            .Should()
            .BeOfType<GameActionResult<AcceptArmutResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.NotYourTurn);
    }

    [Fact]
    public async Task AcceptArmut_Accepted_EntersCardExchangePhase()
    {
        var (repo, pub, id) = await ArmutPartnerFindingGame();
        var useCase = new AcceptArmutHandler(repo, pub);

        await useCase.ExecuteAsync(new AcceptArmutCommand(id, AppB.P1, true));

        var state = await repo.GetAsync(id);
        state!.Phase.Should().Be(GamePhase.ArmutCardExchange);
        state.ArmutRichPlayer.Should().Be(AppB.P1);
        state.CurrentTurn.Should().Be(AppB.P1);
    }

    [Fact]
    public async Task AcceptArmut_Accepted_TransfersTrumpsFromPoorToRich()
    {
        var (repo, pub, id) = await ArmutPartnerFindingGame();
        var useCase = new AcceptArmutHandler(repo, pub);

        await useCase.ExecuteAsync(new AcceptArmutCommand(id, AppB.P1, true));

        var state = await repo.GetAsync(id);
        var poorHand = state!.Players.First(p => p.Id == AppB.P0).Hand;
        var richHand = state.Players.First(p => p.Id == AppB.P1).Hand;

        // Poor player's trumps moved to rich player
        poorHand.Cards.Should().NotContain(Trump1).And.NotContain(Trump2);
        richHand.Cards.Should().Contain(Trump1).And.Contain(Trump2);
        state.ArmutTransferCount.Should().Be(2);
    }

    [Fact]
    public async Task AcceptArmut_Declined_MovesToNextCandidate()
    {
        var (repo, pub, id) = await ArmutPartnerFindingGame();
        var useCase = new AcceptArmutHandler(repo, pub);

        var result = await useCase.ExecuteAsync(new AcceptArmutCommand(id, AppB.P1, false));

        result
            .Should()
            .BeOfType<GameActionResult<AcceptArmutResult>.Ok>()
            .Which.Value.Accepted.Should()
            .BeFalse();
        var state = await repo.GetAsync(id);
        state!.Phase.Should().Be(GamePhase.ArmutPartnerFinding);
        state.CurrentTurn.Should().Be(AppB.P2);
        state.PendingReservationResponders.Should().BeEquivalentTo([AppB.P2, AppB.P3]);
    }

    [Fact]
    public async Task AcceptArmut_AllDeclined_EntersSchwarzesSauMode()
    {
        var (repo, pub, id) = await ArmutPartnerFindingGame();
        var useCase = new AcceptArmutHandler(repo, pub);

        await useCase.ExecuteAsync(new AcceptArmutCommand(id, AppB.P1, false));
        await useCase.ExecuteAsync(new AcceptArmutCommand(id, AppB.P2, false));
        var result = await useCase.ExecuteAsync(new AcceptArmutCommand(id, AppB.P3, false));

        result
            .Should()
            .BeOfType<GameActionResult<AcceptArmutResult>.Ok>()
            .Which.Value.SchwarzesSau.Should()
            .BeTrue();
        var state = await repo.GetAsync(id);
        state!.Phase.Should().Be(GamePhase.Playing);
        // Poor player starts in Schwarze Sau
        state.CurrentTurn.Should().Be(AppB.P0);
    }
}
