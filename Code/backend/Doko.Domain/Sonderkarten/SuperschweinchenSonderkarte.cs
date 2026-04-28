using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Modifications;
using Doko.Domain.Trump;

namespace Doko.Domain.Sonderkarten;

/// <summary>
/// Superschweinchen: requires Schweinchen active; player originally held both ♦ 10s →
/// they rank above the Schweinchen. A ♦ 10 played before Schweinchen was announced is still
/// eligible once the second ♦ 10 is played after Schweinchen activates.
/// </summary>
public sealed class SuperschweinchenSonderkarte : SonderkarteBase
{
    private static readonly CardType KaroZehn = new(Suit.Karo, Rank.Zehn);
    private static readonly CardType KaroAss = new(Suit.Karo, Rank.Ass);

    public override SonderkarteType Type => SonderkarteType.Superschweinchen;
    public override CardType TriggeringCard => KaroZehn;
    public override ISonderkarteRankingModifier RankingModifier =>
        SuperschweinchenModifier.Instance;

    public override bool AreConditionsMet(GameState state) =>
        (IsActive(state, SonderkarteType.Schweinchen) || BothPlayedBySamePlayer(state, KaroAss))
        && !IsActive(state, SonderkarteType.Superschweinchen)
        && !IsWindowClosed(state, SonderkarteType.Superschweinchen)
        && OriginallyHeldBoth(state, KaroZehn);

    protected override GameStateModification? ExtraEffects(
        GameState state,
        ISonderkarteInputProvider inputs
    ) => new RebuildTrumpEvaluatorModification();
}
