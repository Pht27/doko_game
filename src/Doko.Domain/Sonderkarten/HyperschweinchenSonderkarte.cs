using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Trump;

namespace Doko.Domain.Sonderkarten;

/// <summary>
/// Hyperschweinchen: requires Superschweinchen active; player originally held both ♦ Kings →
/// they rank above the Superschweinchen.
/// </summary>
public sealed class HyperschweinchenSonderkarte : SonderkarteBase
{
    private static readonly CardType KaroKoenig = new(Suit.Karo, Rank.Koenig);
    private static readonly CardType KaroZehn  = new(Suit.Karo, Rank.Zehn);

    public override SonderkarteType Type => SonderkarteType.Hyperschweinchen;
    public override CardType TriggeringCard => KaroKoenig;
    public override ISonderkarteRankingModifier RankingModifier => HyperschweinchenModifier.Instance;

    public override bool AreConditionsMet(GameState state)
        => (IsActive(state, SonderkarteType.Superschweinchen) || BothPlayedBySamePlayer(state, KaroZehn))
        && !IsActive(state, SonderkarteType.Hyperschweinchen)
        && !IsWindowClosed(state, SonderkarteType.Hyperschweinchen)
        && OriginallyHeldBoth(state, KaroKoenig);

    protected override GameStateModification? ExtraEffects(GameState state)
        => new RebuildTrumpEvaluatorModification();

}
