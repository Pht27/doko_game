using Doko.Domain.Players;

namespace Doko.Domain.GameFlow.Modifications;

/// <summary>Records one player's health declaration (Gesund/Vorbehalt) during ReservationHealthCheck.</summary>
public sealed record RecordHealthDeclarationModification(PlayerSeat Player, bool HasVorbehalt)
    : GameStateModification;
