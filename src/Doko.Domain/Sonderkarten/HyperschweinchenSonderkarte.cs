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

    public override SonderkarteType Type => SonderkarteType.Hyperschweinchen;
    public override CardType TriggeringCard => KaroKoenig;
    public override ISonderkarteRankingModifier RankingModifier => HyperschweinchenModifier.Instance;

    public override bool AreConditionsMet(GameState state)
        => IsActive(state, SonderkarteType.Superschweinchen)
        && !IsActive(state, SonderkarteType.Hyperschweinchen)
        && OriginallyHeldBoth(state, KaroKoenig);

    public override GameStateModification? Apply(GameState state) => null;
}
