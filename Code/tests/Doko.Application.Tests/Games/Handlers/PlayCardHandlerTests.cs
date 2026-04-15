using Doko.Application.Tests.Helpers;
using Doko.Domain.Announcements;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Parties;
using Doko.Domain.Scoring;
using Doko.Domain.Trump;

namespace Doko.Application.Tests.Games.Handlers;

public class PlayCardHandlerTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Creates a game state already in Playing phase with known hands.</summary>
    private static async Task<(
        Fakes.InMemoryGameRepository repo,
        Fakes.RecordingGameEventPublisher pub,
        GameId id
    )> PlayingGame(
        IReadOnlyList<Card>? p0Hand = null,
        IReadOnlyList<Card>? p1Hand = null,
        IReadOnlyList<Card>? p2Hand = null,
        IReadOnlyList<Card>? p3Hand = null
    )
    {
        var (repo, pub, _) = AppB.Infrastructure();

        // 4 ♦A cards to keep it simple (all trump in normal mode so no follow-suit issues)
        var karoAss0 = AppB.Card(0, Suit.Karo, Rank.Ass);
        var karoAss1 = AppB.Card(1, Suit.Karo, Rank.Ass);
        var pikAss0 = AppB.Card(2, Suit.Pik, Rank.Ass);
        var pikAss1 = AppB.Card(3, Suit.Pik, Rank.Ass);

        p0Hand ??= [karoAss0];
        p1Hand ??= [karoAss1];
        p2Hand ??= [pikAss0];
        p3Hand ??= [pikAss1];

        var players = new[]
        {
            new PlayerState(AppB.P0, PlayerSeat.First, AppB.HandOf([.. p0Hand]), null),
            new PlayerState(AppB.P1, PlayerSeat.Second, AppB.HandOf([.. p1Hand]), null),
            new PlayerState(AppB.P2, PlayerSeat.Third, AppB.HandOf([.. p2Hand]), null),
            new PlayerState(AppB.P3, PlayerSeat.Fourth, AppB.HandOf([.. p3Hand]), null),
        };

        var state = GameState.Create(
            phase: GamePhase.Playing,
            players: players,
            currentTurn: AppB.P0,
            rules: RuleSet.Minimal()
        );

        await repo.SaveAsync(state);
        return (repo, pub, state.Id);
    }

    private static IPlayCardHandler Handler(
        Fakes.InMemoryGameRepository repo,
        Fakes.RecordingGameEventPublisher pub
    ) => new PlayCardHandler(repo, pub, new GameScorer());

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PlayCard_RemovesCardFromHand()
    {
        var card = AppB.Card(0, Suit.Karo, Rank.Ass);
        var (repo, pub, id) = await PlayingGame(p0Hand: [card]);
        var uc = Handler(repo, pub);

        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P0, card.Id, []));

        var state = await repo.GetAsync(id);
        state!.Players.First(p => p.Id == AppB.P0).Hand.Cards.Should().NotContain(card);
    }

    [Fact]
    public async Task PlayCard_AddsCardToCurrentTrick()
    {
        var card = AppB.Card(0, Suit.Karo, Rank.Ass);
        var (repo, pub, id) = await PlayingGame(p0Hand: [card]);
        var uc = Handler(repo, pub);

        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P0, card.Id, []));

        var state = await repo.GetAsync(id);
        state!.CurrentTrick.Should().NotBeNull();
        state.CurrentTrick!.Cards.Should().HaveCount(1);
        state.CurrentTrick.Cards[0].Card.Should().Be(card);
    }

    [Fact]
    public async Task PlayCard_AdvancesTurnToNextPlayer()
    {
        var card = AppB.Card(0, Suit.Karo, Rank.Ass);
        var (repo, pub, id) = await PlayingGame(p0Hand: [card]);
        var uc = Handler(repo, pub);

        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P0, card.Id, []));

        var state = await repo.GetAsync(id);
        state!.CurrentTurn.Should().Be(AppB.P1);
    }

    [Fact]
    public async Task PlayCard_ReturnsNotYourTurn_WhenWrongPlayer()
    {
        var card = AppB.Card(1, Suit.Karo, Rank.Ass);
        var (repo, pub, id) = await PlayingGame(p1Hand: [card]);
        var uc = Handler(repo, pub);

        var result = await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P1, card.Id, []));

        result
            .Should()
            .BeOfType<GameActionResult<PlayCardResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.NotYourTurn);
    }

    [Fact]
    public async Task PlayCard_ReturnsGameNotFound_ForUnknownGame()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        var uc = Handler(repo, pub);

        var result = await uc.ExecuteAsync(
            new PlayCardCommand(GameId.New(), AppB.P0, new CardId(0), [])
        );

        result
            .Should()
            .BeOfType<GameActionResult<PlayCardResult>.Failure>()
            .Which.Error.Should()
            .Be(GameError.GameNotFound);
    }

    [Fact]
    public async Task PlayCard_CompleteTrick_SetsWinnerAsNextTurn()
    {
        // One card each, P0 leads with ♦A (plain), others follow with plain non-♦ → P0's ♦A wins
        var c0 = AppB.Card(0, Suit.Karo, Rank.Ass);
        var c1 = AppB.Card(1, Suit.Karo, Rank.Ass);
        var c2 = AppB.Card(2, Suit.Pik, Rank.Koenig);
        var c3 = AppB.Card(3, Suit.Pik, Rank.Koenig);

        // Use NoTrump so plain-suit rules apply cleanly
        var players = new[]
        {
            new PlayerState(AppB.P0, PlayerSeat.First, AppB.HandOf(c0), null),
            new PlayerState(AppB.P1, PlayerSeat.Second, AppB.HandOf(c1), null),
            new PlayerState(AppB.P2, PlayerSeat.Third, AppB.HandOf(c2), null),
            new PlayerState(AppB.P3, PlayerSeat.Fourth, AppB.HandOf(c3), null),
        };
        var (repo, pub, _) = AppB.Infrastructure();
        var state = GameState.Create(
            phase: GamePhase.Playing,
            players: players,
            currentTurn: AppB.P0,
            trumpEvaluator: NoTrumpEvaluator.Instance,
            rules: RuleSet.Minimal()
        );
        await repo.SaveAsync(state);
        var id = state.Id;
        var uc = Handler(repo, pub);

        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P0, c0.Id, []));
        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P1, c1.Id, []));
        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P2, c2.Id, []));
        var lastResult = await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P3, c3.Id, []));

        var ok = lastResult.Should().BeOfType<GameActionResult<PlayCardResult>.Ok>().Which.Value;
        ok.TrickCompleted.Should().BeTrue();
        // P0 and P1 tied ♦A — first played wins; P0 wins
        ok.TrickWinner.Should().Be(AppB.P0);

        var saved = await repo.GetAsync(id);
        saved!.CompletedTricks.Should().HaveCount(1);
    }

    [Fact]
    public async Task PlayCard_CompleteTrick_AutoMakesPflichtansage_WhenFirstTrickHasHighPoints()
    {
        // P0 leads ♣A (11), P1 plays ♣10 (10), P2 plays ♠A (11), P3 plays ♥A (11) → 43 Augen, P0 wins
        // NoTrump: highest ♣ wins the lead-suit trick
        var c0 = AppB.Card(0, Suit.Kreuz, Rank.Ass);
        var c1 = AppB.Card(1, Suit.Kreuz, Rank.Zehn);
        var c2 = AppB.Card(2, Suit.Pik, Rank.Ass); // no ♣ → can discard
        var c3 = AppB.Card(3, Suit.Herz, Rank.Ass); // no ♣ → can discard

        var players = new[]
        {
            new PlayerState(AppB.P0, PlayerSeat.First, AppB.HandOf(c0), null),
            new PlayerState(AppB.P1, PlayerSeat.Second, AppB.HandOf(c1), null),
            new PlayerState(AppB.P2, PlayerSeat.Third, AppB.HandOf(c2), null),
            new PlayerState(AppB.P3, PlayerSeat.Fourth, AppB.HandOf(c3), null),
        };

        var (repo, pub, _) = AppB.Infrastructure();
        var state = GameState.Create(
            phase: GamePhase.Playing,
            players: players,
            currentTurn: AppB.P0,
            trumpEvaluator: NoTrumpEvaluator.Instance,
            partyResolver: new SoloPartyResolver(AppB.P0), // P0 = Re
            rules: new RuleSet { EnforcePflichtansage = true }
        );
        await repo.SaveAsync(state);
        var id = state.Id;
        var uc = Handler(repo, pub);

        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P0, c0.Id, []));
        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P1, c1.Id, []));
        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P2, c2.Id, []));
        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P3, c3.Id, []));

        // Expect an AnnouncementMadeEvent for Re auto-announced
        pub.Published.OfType<AnnouncementMadeEvent>()
            .Should()
            .ContainSingle(e => e.Player == AppB.P0 && e.Type == AnnouncementType.Win);

        // State should also have the announcement recorded
        var saved = await repo.GetAsync(id);
        saved!
            .Announcements.Should()
            .ContainSingle(a => a.Player == AppB.P0 && a.Type == AnnouncementType.Win);
    }

    [Fact]
    public async Task PlayCard_LastTrick_ReturnsGameFinished()
    {
        // Each player has one card; play until game ends
        var c0 = AppB.Card(0, Suit.Karo, Rank.Ass);
        var c1 = AppB.Card(1, Suit.Kreuz, Rank.Ass);
        var c2 = AppB.Card(2, Suit.Pik, Rank.Ass);
        var c3 = AppB.Card(3, Suit.Herz, Rank.Ass);

        var (repo, pub, id) = await PlayingGame(
            p0Hand: [c0],
            p1Hand: [c1],
            p2Hand: [c2],
            p3Hand: [c3]
        );
        var uc = Handler(repo, pub);

        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P0, c0.Id, []));
        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P1, c1.Id, []));
        await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P2, c2.Id, []));
        var result = await uc.ExecuteAsync(new PlayCardCommand(id, AppB.P3, c3.Id, []));

        var ok = result.Should().BeOfType<GameActionResult<PlayCardResult>.Ok>().Which.Value;
        ok.GameFinished.Should().BeTrue();
        ok.FinishedResult.Should().NotBeNull();

        var saved = await repo.GetAsync(id);
        saved!.Phase.Should().Be(GamePhase.Finished);
    }
}
