using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>Adds a card played by a player to the current trick.</summary>
public sealed record AddCardToTrickModification(PlayerSeat Player, Cards.Card Card)
    : GameStateModification;
