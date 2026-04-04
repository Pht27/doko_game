using Doko.Domain.Cards;
using Doko.Domain.GameFlow;

namespace Doko.Domain.Sonderkarten;

/// <summary>
/// Gegengenscherdamen: requires Genscherdamen active; player originally held both ♦ Queens →
/// may counter-genschern when playing the first ♦ Queen, choosing a new partner.
/// Partner selection is interactive; Apply returns null — the game engine handles the party swap.
/// </summary>
public sealed class GegengenscherdamenSonderkarte : SonderkarteBase
{
    private static readonly CardType KaroDame = new(Suit.Karo, Rank.Dame);

    public override SonderkarteType Type => SonderkarteType.Gegengenscherdamen;
    public override CardType TriggeringCard => KaroDame;

    public override bool AreConditionsMet(GameState state)
        => IsActive(state, SonderkarteType.Genscherdamen)
        && !IsActive(state, SonderkarteType.Gegengenscherdamen)
        && OriginallyHeldBoth(state, KaroDame);

    public override GameStateModification? Apply(GameState state) => null;
}
