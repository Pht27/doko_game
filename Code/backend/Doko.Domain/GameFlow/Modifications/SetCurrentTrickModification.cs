namespace Doko.Domain.GameFlow.Modifications;

/// <summary>Sets the current trick (null clears the current trick after it completes).</summary>
public sealed record SetCurrentTrickModification(Tricks.Trick? Trick) : GameStateModification;
