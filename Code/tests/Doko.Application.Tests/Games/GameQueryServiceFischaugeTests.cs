using Doko.Application.Games;
using Doko.Application.Tests.Fakes;
using Doko.Domain.Players;
using Doko.Domain.Trump;

namespace Doko.Application.Tests.Games;

public class GameQueryServiceFischaugeTests
{
    private static readonly PlayerSeat Requester = PlayerSeat.First;

    private static Card KaroNeun() => new(new CardId(1), new CardType(Suit.Karo, Rank.Neun));

    private static Card KreuzAss() => new(new CardId(2), new CardType(Suit.Kreuz, Rank.Ass));

    private static Card KreuzDame() => new(new CardId(3), new CardType(Suit.Kreuz, Rank.Dame));

    private static Trick TrickWith(params (Card card, PlayerSeat player)[] entries)
    {
        var trick = new Trick();
        foreach (var (card, player) in entries)
            trick.Add(new TrickCard(card, player));
        return trick;
    }

    private static async Task<TrickSummary?> GetCurrentTrickSummary(
        Trick currentTrick,
        IReadOnlyList<Trick>? completedTricks = null
    )
    {
        var player = new PlayerState(Requester, new Hand([]), null);
        var state = GameState.Create(
            phase: GamePhase.Playing,
            players: [player],
            currentTurn: PlayerSeat.Second,
            currentTrick: currentTrick,
            completedTricks: completedTricks
        );

        var repo = new InMemoryGameRepository();
        await repo.SaveAsync(state);

        var service = new GameQueryService(repo);
        var view = await service.GetPlayerViewAsync(state.Id, Requester);
        return view!.CurrentTrick;
    }

    [Fact]
    public async Task Fischauge_KaroNeunAtIndexZero_IsNotFaceDown()
    {
        var completedTrickWithTrump = TrickWith(
            (KreuzDame(), PlayerSeat.First),
            (KreuzAss(), PlayerSeat.Second),
            (KreuzAss(), PlayerSeat.Third),
            (KreuzAss(), PlayerSeat.Fourth)
        );

        var currentTrick = TrickWith((KaroNeun(), PlayerSeat.First));

        var summary = await GetCurrentTrickSummary(
            currentTrick,
            completedTricks: [completedTrickWithTrump]
        );

        summary!.Cards[0].FaceDown.Should().BeFalse();
    }

    [Fact]
    public async Task Fischauge_KaroNeunAtIndexOne_PriorCardIsFehl_IsFaceDown()
    {
        var completedTrickWithTrump = TrickWith(
            (KreuzDame(), PlayerSeat.First),
            (KreuzAss(), PlayerSeat.Second),
            (KreuzAss(), PlayerSeat.Third),
            (KreuzAss(), PlayerSeat.Fourth)
        );

        // KreuzAss is Fehl (not trump under NormalTrumpEvaluator)
        var currentTrick = TrickWith(
            (KreuzAss(), PlayerSeat.Second),
            (KaroNeun(), PlayerSeat.Third)
        );

        var summary = await GetCurrentTrickSummary(
            currentTrick,
            completedTricks: [completedTrickWithTrump]
        );

        summary!.Cards[1].FaceDown.Should().BeTrue();
    }

    [Fact]
    public async Task Fischauge_KaroNeunAtIndexOne_PriorCardIsTrump_IsNotFaceDown()
    {
        var completedTrickWithTrump = TrickWith(
            (KreuzDame(), PlayerSeat.First),
            (KreuzAss(), PlayerSeat.Second),
            (KreuzAss(), PlayerSeat.Third),
            (KreuzAss(), PlayerSeat.Fourth)
        );

        // KreuzDame is trump
        var currentTrick = TrickWith(
            (KreuzDame(), PlayerSeat.Second),
            (KaroNeun(), PlayerSeat.Third)
        );

        var summary = await GetCurrentTrickSummary(
            currentTrick,
            completedTricks: [completedTrickWithTrump]
        );

        summary!.Cards[1].FaceDown.Should().BeFalse();
    }

    [Fact]
    public async Task Fischauge_FischaugeNotActive_KaroNeunIsNotFaceDown()
    {
        // No completed tricks with trump → fischaugeActive = false
        var currentTrick = TrickWith(
            (KreuzAss(), PlayerSeat.Second),
            (KaroNeun(), PlayerSeat.Third)
        );

        var summary = await GetCurrentTrickSummary(currentTrick, completedTricks: []);

        summary!.Cards[1].FaceDown.Should().BeFalse();
    }
}
