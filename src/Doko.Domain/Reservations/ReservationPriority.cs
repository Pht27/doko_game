namespace Doko.Domain.Reservations;

/// <summary>
/// Determines which reservation wins when two players declare the same priority level.
/// Lower value = higher priority (pre-empts higher values).
/// Farbsolo sub-order: ♦ > ♣ > ♠ > ♥; Schlanker Martin is lowest within Soli.
/// </summary>
public enum ReservationPriority
{
    // ── Soli (highest overall) ──────────────────────────────
    KaroSolo       = 0,
    KreuzSolo      = 1,
    PikSolo        = 2,
    HerzSolo       = 3,
    Damensolo      = 4,
    Bubensolo      = 5,
    Fleischloses   = 6,
    Knochenloses   = 7,
    SchlankerMartin = 8,

    // ── Other reservations ──────────────────────────────────
    Armut      = 9,
    Hochzeit   = 10,
    Schmeissen = 11,
}
