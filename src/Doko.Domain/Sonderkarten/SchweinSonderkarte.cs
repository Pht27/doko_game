using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Trump;

namespace Doko.Domain.Sonderkarten;

/// <summary>
/// Schweinchen: player originally held both ♦ Aces → they become the two highest trumps (above Dullen).
/// Announced when playing the first ♦ Ace. The ranking effect is applied via SchweinchenModifier
/// once this sonderkarte appears in GameState.ActiveSonderkarten.
/// </summary>
public sealed class SchweinSonderkarte : SonderkarteBase
{
    private static readonly CardType KaroAss = new(Suit.Karo, Rank.Ass);

    public override SonderkarteType Type => SonderkarteType.Schweinchen;
    public override CardType TriggeringCard => KaroAss;
    public override ISonderkarteRankingModifier RankingModifier => SchweinchenModifier.Instance;

    public override bool AreConditionsMet(GameState state)
        => !IsActive(state, SonderkarteType.Schweinchen)
        && !IsWindowClosed(state, SonderkarteType.Schweinchen)
        && OriginallyHeldBoth(state, KaroAss);

    // Ranking change is applied by the game engine via SchweinchenModifier once active.
    public override GameStateModification? Apply(GameState state) => null;
}
