using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>Replaces the list of players still awaiting a response in the current reservation check phase.</summary>
public sealed record SetPendingRespondersModification(IReadOnlyList<PlayerSeat> Responders)
    : GameStateModification;
