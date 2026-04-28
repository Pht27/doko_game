using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Modifications;

namespace Doko.Domain.Sonderkarten;

/// <summary>
/// Linksdrehender Gehängter: player originally held both ♦ Jacks → when playing the first ♦ Jack,
/// may announce and reverse the play direction. Direction change takes effect immediately if led,
/// otherwise from the next trick (handled by the game engine).
/// </summary>
public sealed class LinksGehangterSonderkarte : SonderkarteBase
{
    private static readonly CardType KaroBube = new(Suit.Karo, Rank.Bube);

    public override SonderkarteType Type => SonderkarteType.LinksGehangter;
    public override CardType TriggeringCard => KaroBube;

    public override bool WindowClosesWhenDeclined => false;

    public override bool AreConditionsMet(GameState state) =>
        !IsActive(state, SonderkarteType.LinksGehangter) && OriginallyHeldBoth(state, KaroBube);

    protected override GameStateModification? ExtraEffects(
        GameState state,
        ISonderkarteInputProvider inputs
    ) =>
        state.CurrentTrick is null || state.CurrentTrick.Cards.Count == 0
            ? new ReverseDirectionModification()
            : new ScheduleDirectionFlipModification();
}
