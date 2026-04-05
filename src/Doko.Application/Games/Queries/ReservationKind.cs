namespace Doko.Application.Games.Queries;

/// <summary>
/// Identifies a type of reservation a player may declare, for display purposes.
/// Used in <see cref="PlayerGameView.EligibleReservations"/> to let clients show
/// only the options the player is actually eligible for.
/// </summary>
public enum ReservationKind
{
    Hochzeit,
    Armut,
    Schmeissen,
    Damensolo,
    Bubensolo,
    Fleischloses,
    Knochenloses,
    SchlankerMartin,
    KaroSolo,
    KreuzSolo,
    PikSolo,
    HerzSolo,
}
