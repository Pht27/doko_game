namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Discards all announcements made so far.
/// Applied unconditionally when a Schwarze-Sau solo is chosen — announcements from the
/// Normalspiel phase carry no meaning under the new solo's party structure.
/// </summary>
public sealed record ClearAnnouncementsModification : GameStateModification;
