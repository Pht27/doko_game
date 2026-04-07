using Doko.Application.Tests.Helpers;

namespace Doko.Application.Tests.Games.UseCases;

public class ExchangeArmutCardsUseCaseTests
{
    private static readonly Card Trump1 = AppB.Card(0, Suit.Kreuz, Rank.Bube);
    private static readonly Card Trump2 = AppB.Card(1, Suit.Kreuz, Rank.Dame);
    private static readonly Card NonTrump0 = AppB.Card(2, Suit.Kreuz, Rank.Koenig);
    private static readonly Card NonTrump1 = AppB.Card(3, Suit.Pik, Rank.Koenig);
    private static readonly Card NonTrump2 = AppB.Card(4, Suit.Pik, Rank.Ass);
    private static readonly Card NonTrump3 = AppB.Card(5, Suit.Herz, Rank.Koenig);

    /// <summary>
    /// Creates a game in <see cref="GamePhase.ArmutCardExchange"/>.
    /// P0 is the poor player (holds only NonTrump0); P1 is the rich player
    /// and holds Trump1, Trump2 (received from P0) plus NonTrump1.
    /// ArmutTransferCount = 2.
    /// </summary>
    private static async Task<(
        Fakes.InMemoryGameRepository repo,
        Fakes.RecordingGameEventPublisher pub,
        GameId id
    )> ArmutCardExchangeGame()
    {
        var (repo, pub, _) = AppB.Infrastructure();

        // Start in ArmutPartnerFinding so ArmutGiveTrumpsModification can fire
        var players = new[]
        {
            new PlayerState(AppB.P0, PlayerSeat.First, AppB.HandOf(Trump1, Trump2, NonTrump0), null),
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

        // P1 accepts → game moves to ArmutCardExchange, trumps transferred
        await new AcceptArmutUseCase(repo, pub).ExecuteAsync(
            new AcceptArmutCommand(state.Id, AppB.P1, true)
        );

        return (repo, pub, state.Id);
    }

    [Fact]
    public async Task ExchangeArmutCards_WrongPhase_ReturnsInvalidPhase()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var state = GameState.Create(phase: GamePhase.Playing, currentTurn: AppB.P1);
        state.Apply(new SetArmutRichPlayerModification(AppB.P1));
        await repo.SaveAsync(state);
        var useCase = new ExchangeArmutCardsUseCase(repo, pub);

        var result = await useCase.ExecuteAsync(
            new ExchangeArmutCardsCommand(state.Id, AppB.P1, [])
        );

        result
            .Should()
            .BeOfType<GameActionResult<ExchangeArmutCardsResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.InvalidPhase);
    }

    [Fact]
    public async Task ExchangeArmutCards_WrongPlayer_ReturnsNotYourTurn()
    {
        var (repo, pub, id) = await ArmutCardExchangeGame();
        var useCase = new ExchangeArmutCardsUseCase(repo, pub);

        // P0 (poor player) tries to return cards — only the rich player can
        var result = await useCase.ExecuteAsync(
            new ExchangeArmutCardsCommand(id, AppB.P0, [Trump1.Id, Trump2.Id])
        );

        result
            .Should()
            .BeOfType<GameActionResult<ExchangeArmutCardsResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.NotYourTurn);
    }

    [Fact]
    public async Task ExchangeArmutCards_WrongCount_ReturnsIllegalCard()
    {
        var (repo, pub, id) = await ArmutCardExchangeGame();
        var useCase = new ExchangeArmutCardsUseCase(repo, pub);

        // ArmutTransferCount = 2; returning only 1 card is invalid
        var result = await useCase.ExecuteAsync(
            new ExchangeArmutCardsCommand(id, AppB.P1, [Trump1.Id])
        );

        result
            .Should()
            .BeOfType<GameActionResult<ExchangeArmutCardsResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.IllegalCard);
    }

    [Fact]
    public async Task ExchangeArmutCards_CardNotInHand_ReturnsIllegalCard()
    {
        var (repo, pub, id) = await ArmutCardExchangeGame();
        var useCase = new ExchangeArmutCardsUseCase(repo, pub);

        // NonTrump2 belongs to P2, not P1
        var result = await useCase.ExecuteAsync(
            new ExchangeArmutCardsCommand(id, AppB.P1, [Trump1.Id, NonTrump2.Id])
        );

        result
            .Should()
            .BeOfType<GameActionResult<ExchangeArmutCardsResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.IllegalCard);
    }

    [Fact]
    public async Task ExchangeArmutCards_ValidExchange_AdvancesToPlaying()
    {
        var (repo, pub, id) = await ArmutCardExchangeGame();
        var useCase = new ExchangeArmutCardsUseCase(repo, pub);

        // Return the 2 trumps received from poor player
        var result = await useCase.ExecuteAsync(
            new ExchangeArmutCardsCommand(id, AppB.P1, [Trump1.Id, Trump2.Id])
        );

        result.Should().BeOfType<GameActionResult<ExchangeArmutCardsResult>.Ok>();
        var state = await repo.GetAsync(id);
        state!.Phase.Should().Be(GamePhase.Playing);
    }

    [Fact]
    public async Task ExchangeArmutCards_ValidExchange_UpdatesBothHands()
    {
        var (repo, pub, id) = await ArmutCardExchangeGame();
        var useCase = new ExchangeArmutCardsUseCase(repo, pub);

        // Rich player (P1) returns Trump1 and Trump2 to poor player (P0)
        await useCase.ExecuteAsync(
            new ExchangeArmutCardsCommand(id, AppB.P1, [Trump1.Id, Trump2.Id])
        );

        var state = await repo.GetAsync(id);
        var poorHand = state!.Players.First(p => p.Id == AppB.P0).Hand;
        var richHand = state.Players.First(p => p.Id == AppB.P1).Hand;

        poorHand.Cards.Should().Contain(Trump1).And.Contain(Trump2);
        richHand.Cards.Should().NotContain(Trump1).And.NotContain(Trump2);
    }

    [Fact]
    public async Task ExchangeArmutCards_ValidExchange_FirstNonRichPartyPlayerLeftOfRichLeads()
    {
        // P0 = poor (seat 0), P1 = rich (seat 1)
        // Left of rich: P2 (seat 2), then P3 (seat 3), then P0 (poor — skip).
        // Expected leader: P2.
        var (repo, pub, id) = await ArmutCardExchangeGame();
        var useCase = new ExchangeArmutCardsUseCase(repo, pub);

        await useCase.ExecuteAsync(new ExchangeArmutCardsCommand(id, AppB.P1, [Trump1.Id, Trump2.Id]));

        var state = await repo.GetAsync(id);
        state!.CurrentTurn.Should().Be(AppB.P2);
    }

    [Fact]
    public async Task ExchangeArmutCards_ReturnedTrumpCount_ReflectsActualTrumpsReturned()
    {
        var (repo, pub, id) = await ArmutCardExchangeGame();
        var useCase = new ExchangeArmutCardsUseCase(repo, pub);

        // Return 1 trump (Trump1) and 1 non-trump (NonTrump1 is in P1's hand)
        var result = await useCase.ExecuteAsync(
            new ExchangeArmutCardsCommand(id, AppB.P1, [Trump1.Id, NonTrump1.Id])
        );

        result
            .Should()
            .BeOfType<GameActionResult<ExchangeArmutCardsResult>.Ok>()
            .Which.Value.ReturnedTrumpCount.Should()
            .Be(1);
    }
}
