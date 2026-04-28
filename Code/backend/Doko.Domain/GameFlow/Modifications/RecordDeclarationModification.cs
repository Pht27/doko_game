using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>Records one player's reservation declaration during the reservation phase.</summary>
public sealed record RecordDeclarationModification(PlayerSeat Player, IReservation? Declaration)
    : GameStateModification;
