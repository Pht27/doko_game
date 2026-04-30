using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Modifications;

namespace Doko.Domain.Sonderkarten;

/// <summary>
/// Heidfrau: requires Heidmann active; player originally held both ♠ Queens →
/// when playing the next ♠ Queen, may choose to reverse the Heidmann effect (Queens back above Jacks).
/// The game engine removes HeidmannModifier from the TrumpEvaluator when Heidfrau activates.
/// </summary>
public sealed class HeidfrauSonderkarte : SonderkarteBase
{
    private static readonly CardType PikDame = new(Suit.Pik, Rank.Dame);

    public override SonderkarteType Type => SonderkarteType.Heidfrau;
    public override CardType TriggeringCard => PikDame;
    public override SonderkarteType? Suppresses => SonderkarteType.Heidmann;

    public override bool AreConditionsMet(PlayingState state) =>
        IsActive(state, SonderkarteType.Heidmann)
        && !IsActive(state, SonderkarteType.Heidfrau)
        && !IsWindowClosed(state, SonderkarteType.Heidfrau)
        && OriginallyHeldBoth(state, PikDame);

    protected override GameStateModification? ExtraEffects(
        GameState state,
        ISonderkarteInputProvider inputs
    ) => new RebuildTrumpEvaluatorModification();
}
