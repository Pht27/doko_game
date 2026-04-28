using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Records that a Genscher (Genscherdamen or Gegengenscherdamen) was activated and the
/// playing player has chosen a partner. GameState.Apply creates the GenscherPartyResolver
/// internally, so the Application layer does not need to know about it.
/// </summary>
public sealed record SetGenscherPartnerModification(PlayerSeat Genscher, PlayerSeat Partner)
    : GameStateModification;
