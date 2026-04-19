using Doko.Domain.Players;

namespace Doko.Domain.GameFlow;

public enum SilentGameModeType
{
    KontraSolo,
    StilleHochzeit,
}

/// <summary>
/// A silent (undeclared) game mode active for a specific player.
/// Kontrasolo: player holds both ♠ Queens and both ♠ Kings — mandatory solo as Kontra.
/// StilleHochzeit: player holds both ♣ Queens and did not declare Hochzeit — silent solo as Re.
/// </summary>
public sealed record SilentGameMode(SilentGameModeType Type, PlayerSeat Player);
