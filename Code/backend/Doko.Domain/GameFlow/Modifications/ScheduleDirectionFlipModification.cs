namespace Doko.Domain.GameFlow.Modifications;

/// <summary>
/// Schedules a direction reversal to take effect at the start of the next trick.
/// Used when LinksGehangter/RechtsGehangter fires on a non-lead card mid-trick.
/// If the card IS the trick lead, <see cref="PlayCardHandler"/> applies
/// <see cref="ReverseDirectionModification"/> immediately instead.
/// </summary>
public sealed record ScheduleDirectionFlipModification : GameStateModification;
