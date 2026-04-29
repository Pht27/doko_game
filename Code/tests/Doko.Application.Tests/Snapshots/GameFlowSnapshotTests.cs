using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Doko.Application.Tests.Fakes;
using Doko.Application.Tests.Helpers;
using Doko.Domain.GameFlow.Modifications;
using Doko.Domain.Scoring;

namespace Doko.Application.Tests.Snapshots;

/// <summary>
/// Baseline snapshot tests that capture <see cref="PlayerGameView"/> (for player 0) after each
/// major game-flow scenario completes.
///
/// Purpose: these tests catch structural regressions during the GameState-to-record migration.
/// Baselines are stored as .json files next to this file. Set environment variable
/// UPDATE_SNAPSHOTS=true to regenerate them.
/// </summary>
public partial class GameFlowSnapshotTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private static string Serialize(object value) =>
        JsonSerializer.Serialize(value, value.GetType(), JsonOptions);

    private static string BaselineDir =>
        Path.Combine(
            Path.GetDirectoryName(typeof(GameFlowSnapshotTests).Assembly.Location)!,
            "Snapshots",
            "Baselines"
        );

    private static string BaselinePath(string name) =>
        Path.Combine(BaselineDir, $"{name}.json");

    private static bool UpdateMode =>
        Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS") == "true";

    [GeneratedRegex(
        @"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex GuidPattern();

    private static string Normalize(string json) =>
        GuidPattern().Replace(json, "00000000-0000-0000-0000-000000000000");

    private static void AssertMatchesBaseline(string actual, string name)
    {
        var normalized = Normalize(actual);
        var path = BaselinePath(name);
        if (UpdateMode || !File.Exists(path))
        {
            Directory.CreateDirectory(BaselineDir);
            File.WriteAllText(path, normalized);
            if (!UpdateMode)
                Assert.Fail(
                    $"Baseline '{name}.json' did not exist — created it. Re-run the test to verify."
                );
            return;
        }

        var baseline = File.ReadAllText(path);
        Assert.Equal(baseline.ReplaceLineEndings("\n"), normalized.ReplaceLineEndings("\n"));
    }

    private static (
        InMemoryGameRepository repo,
        RecordingGameEventPublisher pub
    ) Infra()
    {
        var (repo, pub, _) = AppB.Infrastructure();
        return (repo, pub);
    }

    private static IPlayCardHandler PlayHandler(
        InMemoryGameRepository repo,
        RecordingGameEventPublisher pub
    ) => new PlayCardHandler(repo, pub, new FinishGameHandler(new GameScorer()));

    private static async Task<PlayerGameView?> GetViewAsync(
        InMemoryGameRepository repo,
        GameId id,
        PlayerSeat player = PlayerSeat.First
    )
    {
        var queryService = new Doko.Application.Games.GameQueryService(repo);
        return await queryService.GetPlayerViewAsync(id, player);
    }

    /// <summary>
    /// Plays all cards to completion using legal cards from the query service.
    /// Returns the final GameState phase.
    /// </summary>
    private static async Task<GamePhase> PlayToCompletion(
        InMemoryGameRepository repo,
        RecordingGameEventPublisher pub,
        GameId id
    )
    {
        var handler = PlayHandler(repo, pub);
        var queryService = new Doko.Application.Games.GameQueryService(repo);

        for (int i = 0; i < 500; i++)
        {
            var s = await repo.GetAsync(id);
            if (s!.Phase != GamePhase.Playing)
                return s.Phase;

            var current = s.CurrentTurn;
            var view = await queryService.GetPlayerViewAsync(id, current);
            if (view?.LegalCards is not { Count: > 0 })
                break;

            var card = view.LegalCards.First();
            var result = await handler.ExecuteAsync(new PlayCardCommand(id, current, card.Id, []));
            if (result is GameActionResult<PlayCardResult>.Failure f)
                throw new InvalidOperationException(
                    $"PlayCard failed: {f.Error} (seat={current}, card={card.Id})"
                );
        }

        return (await repo.GetAsync(id))!.Phase;
    }

    // ── Scenario 1: Normalspiel ───────────────────────────────────────────────
    // 4×12 all-trump (♦9) cards, Playing phase directly, SoloPartyResolver.

    [Fact]
    public async Task Snapshot_Normalspiel_AllGesund_Finished()
    {
        var (repo, pub) = Infra();

        static Card KaroNeun(byte id) => AppB.Card(id, Suit.Karo, Rank.Neun);
        var players = Enumerable
            .Range(0, 4)
            .Select(seat =>
                new PlayerState(
                    (PlayerSeat)seat,
                    AppB.HandOf(
                        Enumerable.Range(seat * 12, 12).Select(i => KaroNeun((byte)i)).ToArray()
                    ),
                    null
                )
            )
            .ToList();

        var state = GameState.Create(
            phase: GamePhase.Playing,
            players: players,
            currentTurn: AppB.P0,
            rules: RuleSet.Minimal(),
            partyResolver: new SoloPartyResolver(AppB.P0)
        );
        await repo.SaveAsync(state);

        var finalPhase = await PlayToCompletion(repo, pub, state.Id);
        Assert.Equal(GamePhase.Finished, finalPhase);

        var view = await GetViewAsync(repo, state.Id);
        Assert.NotNull(view);
        AssertMatchesBaseline(Serialize(view), "Normalspiel");
    }

    // ── Scenario 2: Armut ────────────────────────────────────────────────────
    // P0 has 2 trumps + 8 ♠A; P1–P3 have 10 ♠A each. RuleSet.Minimal (10 tricks).

    [Fact]
    public async Task Snapshot_Armut_DeclareAcceptExchangePlay_Finished()
    {
        var (repo, pub) = Infra();

        static Card PikAss(byte id) => AppB.Card(id, Suit.Pik, Rank.Ass);

        var players = new[]
        {
            new PlayerState(
                AppB.P0,
                AppB.HandOf(
                    [
                        AppB.Card(0, Suit.Kreuz, Rank.Bube),  // trump
                        AppB.Card(1, Suit.Kreuz, Rank.Dame),  // trump
                        .. Enumerable.Range(2, 8).Select(i => PikAss((byte)i)),
                    ]
                ),
                null
            ),
            new PlayerState(
                AppB.P1,
                AppB.HandOf(Enumerable.Range(20, 10).Select(i => PikAss((byte)i)).ToArray()),
                null
            ),
            new PlayerState(
                AppB.P2,
                AppB.HandOf(Enumerable.Range(40, 10).Select(i => PikAss((byte)i)).ToArray()),
                null
            ),
            new PlayerState(
                AppB.P3,
                AppB.HandOf(Enumerable.Range(60, 10).Select(i => PikAss((byte)i)).ToArray()),
                null
            ),
        };

        var state = GameState.Create(
            phase: GamePhase.ArmutPartnerFinding,
            players: players,
            currentTurn: AppB.P1,
            rules: RuleSet.Minimal()
        );
        state.Apply(new SetArmutPlayerModification(AppB.P0));
        state.Apply(new SetPendingRespondersModification([AppB.P1, AppB.P2, AppB.P3]));
        await repo.SaveAsync(state);
        var id = state.Id;

        await new AcceptArmutHandler(repo, pub).ExecuteAsync(
            new AcceptArmutCommand(id, AppB.P1, true)
        );

        var afterAccept = await repo.GetAsync(id);
        var richHand = afterAccept!.Players.First(p => p.Seat == AppB.P1).Hand.Cards;
        var cardsToReturn = richHand
            .Take(afterAccept.Armut!.TransferCount)
            .Select(c => c.Id)
            .ToList();
        var exchangeResult = await new ExchangeArmutCardsHandler(repo, pub).ExecuteAsync(
            new ExchangeArmutCardsCommand(id, AppB.P1, cardsToReturn)
        );
        Assert.IsType<GameActionResult<ExchangeArmutCardsResult>.Ok>(exchangeResult);

        var finalPhase = await PlayToCompletion(repo, pub, id);
        Assert.Equal(GamePhase.Finished, finalPhase);

        var view = await GetViewAsync(repo, id);
        Assert.NotNull(view);
        AssertMatchesBaseline(Serialize(view), "Armut");
    }

    // ── Scenario 3: Solo ─────────────────────────────────────────────────────
    // Playing state with BubensoloReservation pre-set. 4×12 all-trump cards.

    [Fact]
    public async Task Snapshot_Solo_DeclareAndPlay_Finished()
    {
        var (repo, pub) = Infra();

        static Card KaroNeun(byte id) => AppB.Card(id, Suit.Karo, Rank.Neun);
        var players = Enumerable
            .Range(0, 4)
            .Select(seat =>
                new PlayerState(
                    (PlayerSeat)seat,
                    AppB.HandOf(
                        Enumerable.Range(seat * 12, 12).Select(i => KaroNeun((byte)i)).ToArray()
                    ),
                    null
                )
            )
            .ToList();

        var soloReservation = new BubensoloReservation(AppB.P0);
        var ctx = soloReservation.BuildContext();
        var state = GameState.Create(
            phase: GamePhase.Playing,
            players: players,
            currentTurn: AppB.P0,
            rules: RuleSet.Default(),
            activeReservation: soloReservation,
            partyResolver: ctx.PartyResolver,
            trumpEvaluator: ctx.TrumpEvaluator
        );
        state.Apply(new SetGameModeModification(soloReservation, AppB.P0));
        await repo.SaveAsync(state);

        var finalPhase = await PlayToCompletion(repo, pub, state.Id);
        Assert.Equal(GamePhase.Finished, finalPhase);

        var view = await GetViewAsync(repo, state.Id);
        Assert.NotNull(view);
        AssertMatchesBaseline(Serialize(view), "Solo");
    }

    // ── Scenario 4: Hochzeit ─────────────────────────────────────────────────
    // P0 has both ♣Q; others have 2 all-trump cards each.

    [Fact]
    public async Task Snapshot_Hochzeit_PartnerFound_Finished()
    {
        var (repo, pub) = Infra();

        static Card KaroNeun(byte id) => AppB.Card(id, Suit.Karo, Rank.Neun);

        var players = new[]
        {
            new PlayerState(
                AppB.P0,
                AppB.HandOf(
                    AppB.Card(0, Suit.Kreuz, Rank.Dame),
                    AppB.Card(1, Suit.Kreuz, Rank.Dame)
                ),
                null
            ),
            new PlayerState(
                AppB.P1,
                AppB.HandOf(Enumerable.Range(10, 2).Select(i => KaroNeun((byte)i)).ToArray()),
                null
            ),
            new PlayerState(
                AppB.P2,
                AppB.HandOf(Enumerable.Range(20, 2).Select(i => KaroNeun((byte)i)).ToArray()),
                null
            ),
            new PlayerState(
                AppB.P3,
                AppB.HandOf(Enumerable.Range(30, 2).Select(i => KaroNeun((byte)i)).ToArray()),
                null
            ),
        };

        var hochzeitReservation = new HochzeitReservation(AppB.P0, HochzeitCondition.FirstTrick);
        var state = GameState.Create(
            phase: GamePhase.Playing,
            players: players,
            currentTurn: AppB.P0,
            rules: RuleSet.Minimal(),
            activeReservation: hochzeitReservation,
            partyResolver: new HochzeitPartyResolver(AppB.P0, HochzeitCondition.FirstTrick)
        );
        await repo.SaveAsync(state);

        var finalPhase = await PlayToCompletion(repo, pub, state.Id);
        Assert.Equal(GamePhase.Finished, finalPhase);

        var view = await GetViewAsync(repo, state.Id);
        Assert.NotNull(view);
        AssertMatchesBaseline(Serialize(view), "Hochzeit");
    }
}
