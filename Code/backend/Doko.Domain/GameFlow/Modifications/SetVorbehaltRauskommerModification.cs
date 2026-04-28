using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Records the VorbehaltRauskommer — the player who leads the reservation-check ordering
/// for this round. Set once at deal time; used by MakeReservationHandler to pick
/// who plays the first card in Normal/Hochzeit/SchlankerMartin games.
/// </summary>
public sealed record SetVorbehaltRauskommerModification(PlayerSeat Player) : GameStateModification;
