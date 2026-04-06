using Doko.Application.Tests.Fakes;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Scoring;

namespace Doko.Application.Tests.Helpers;

/// <summary>Builder helpers for application-layer tests.</summary>
internal static class AppB
{
    public static readonly PlayerId P0 = new(0);
    public static readonly PlayerId P1 = new(1);
    public static readonly PlayerId P2 = new(2);
    public static readonly PlayerId P3 = new(3);

    public static IReadOnlyList<PlayerId> FourPlayerIds => [P0, P1, P2, P3];

    // ── Fixture factory ───────────────────────────────────────────────────────
    public static (
        InMemoryGameRepository repo,
        RecordingGameEventPublisher pub,
        FakeDeckShuffler shuffler
    ) Infrastructure() =>
        (new InMemoryGameRepository(), new RecordingGameEventPublisher(), new FakeDeckShuffler());

    // ── Convenience ──────────────────────────────────────────────────────────
    public static Card Card(byte id, Suit suit, Rank rank) =>
        new(new CardId(id), new CardType(suit, rank));

    public static Hand HandOf(params Card[] cards) => new(cards);

    public static GameState StateInPhase(
        GamePhase phase,
        PlayerId currentTurn = default,
        IReadOnlyList<PlayerState>? players = null
    ) =>
        GameState.Create(
            phase: phase,
            currentTurn: currentTurn.Value == 0 ? P0 : currentTurn,
            players: players
        );
}
