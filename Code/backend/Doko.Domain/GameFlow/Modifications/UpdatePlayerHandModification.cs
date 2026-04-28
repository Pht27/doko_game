using Doko.Domain.Hands;
using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>Replaces a player's hand with a new hand (e.g. after playing a card).</summary>
public sealed record UpdatePlayerHandModification(PlayerSeat Player, Hand NewHand)
    : GameStateModification;
