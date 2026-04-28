namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Sets a silent (undeclared) game mode — Kontrasolo or Stille Hochzeit.
/// Applied in the all-Gesund path when no reservation was declared.
/// Null clears any active silent mode (fallback to normal game).
/// </summary>
public sealed record SetSilentGameModeModification(SilentGameMode? Mode) : GameStateModification;
