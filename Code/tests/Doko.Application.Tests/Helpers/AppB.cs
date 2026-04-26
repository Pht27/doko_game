using Doko.Application.Tests.Fakes;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Scoring;

namespace Doko.Application.Tests.Helpers;

/// <summary>Builder helpers for application-layer tests.</summary>
internal static class AppB
{
    public static readonly PlayerSeat P0 = PlayerSeat.First;
    public static readonly PlayerSeat P1 = PlayerSeat.Second;
    public static readonly PlayerSeat P2 = PlayerSeat.Third;
    public static readonly PlayerSeat P3 = PlayerSeat.Fourth;

    public static IReadOnlyList<PlayerSeat> FourPlayerSeats => [P0, P1, P2, P3];

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
        PlayerSeat currentTurn = default,
        IReadOnlyList<PlayerState>? players = null
    ) =>
        GameState.Create(
            phase: phase,
            currentTurn: currentTurn == default ? P0 : currentTurn,
            players: players
        );
}
