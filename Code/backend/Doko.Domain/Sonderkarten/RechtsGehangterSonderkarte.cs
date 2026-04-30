using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Modifications;

namespace Doko.Domain.Sonderkarten;

/// <summary>
/// Rechtsdrehender Gehängter: requires LinksGehangter active; player originally held both ♦ Jacks →
/// when playing the second ♦ Jack, may announce and reverse the direction again.
/// </summary>
public sealed class RechtsGehangterSonderkarte : SonderkarteBase
{
    private static readonly CardType KaroBube = new(Suit.Karo, Rank.Bube);

    public override SonderkarteType Type => SonderkarteType.RechtsGehangter;
    public override CardType TriggeringCard => KaroBube;

    public override bool AreConditionsMet(PlayingState state) =>
        IsActive(state, SonderkarteType.LinksGehangter)
        && !IsActive(state, SonderkarteType.RechtsGehangter)
        && OriginallyHeldBoth(state, KaroBube);

    protected override GameStateModification? ExtraEffects(
        GameState state,
        ISonderkarteInputProvider inputs
    )
    {
        var currentTrick = state is PlayingState p ? p.CurrentTrick : null;
        return currentTrick is null || currentTrick.Cards.Count == 0
            ? new ReverseDirectionModification()
            : new ScheduleDirectionFlipModification();
    }
}
