using Doko.Domain.Hands;
using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>Deals hands to all players and records the initial hand snapshot.</summary>
public sealed record DealHandsModification(IReadOnlyDictionary<PlayerSeat, Hand> Hands)
    : GameStateModification;
