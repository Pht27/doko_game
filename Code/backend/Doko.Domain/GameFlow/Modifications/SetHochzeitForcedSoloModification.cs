namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Flags that an announced Hochzeit failed to find a partner in 3 qualifying tricks,
/// turning the game into a forced solo (same party structure as Stille Hochzeit but with
/// Sonderkarten and Extrapunkte active, and scored with soloFactor=3).
/// </summary>
public sealed record SetHochzeitForcedSoloModification : GameStateModification;
