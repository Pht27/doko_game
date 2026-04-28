using Doko.Domain.Sonderkarten;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Marks the activation window for a sonderkarte as permanently closed.
/// Applied when a player plays the triggering card but does not activate an eligible sonderkarte
/// whose <c>WindowClosesWhenDeclined</c> is true.
/// </summary>
public sealed record CloseActivationWindowModification(SonderkarteType Type)
    : GameStateModification;
