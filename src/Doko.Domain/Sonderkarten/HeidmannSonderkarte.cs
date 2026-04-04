using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Trump;

namespace Doko.Domain.Sonderkarten;

/// <summary>
/// Heidmann: player originally held both ♠ Jacks → when playing the FIRST ♠ Jack,
/// may announce "Heidmann" — Jacks now rank above Queens.
/// Must announce on the first ♠ Jack; the opportunity expires if not taken.
/// The ranking effect is applied via HeidmannModifier once active in GameState.ActiveSonderkarten.
/// </summary>
public sealed class HeidmannSonderkarte : SonderkarteBase
{
    private static readonly CardType PikBube = new(Suit.Pik, Rank.Bube);

    public override SonderkarteType Type => SonderkarteType.Heidmann;
    public override CardType TriggeringCard => PikBube;
    public override ISonderkarteRankingModifier RankingModifier => HeidmannModifier.Instance;

    public override bool AreConditionsMet(GameState state)
    {
        if (IsActive(state, SonderkarteType.Heidmann)) return false;
        if (!OriginallyHeldBoth(state, PikBube)) return false;

        // The window expires once the player has already played a ♠ Jack
        bool alreadyPlayedPikBube = state.CompletedTricks
            .SelectMany(t => t.Cards)
            .Any(tc => tc.Player == state.CurrentTurn && tc.Card.Type == PikBube);

        return !alreadyPlayedPikBube;
    }

    public override GameStateModification? Apply(GameState state) => null;
}
