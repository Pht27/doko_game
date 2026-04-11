using Doko.Domain.Cards;
using Doko.Domain.GameFlow;

namespace Doko.Domain.Sonderkarten;

/// <summary>
/// Genscherdamen: player originally held both ♥ Queens → when playing the first ♥ Queen,
/// may announce "Genschern" and choose a new partner (making them the Re party).
/// Partner selection is interactive; Apply returns null — the game engine handles the party swap.
/// </summary>
public sealed class GenscherdamenSonderkarte : SonderkarteBase
{
    private static readonly CardType HerzDame = new(Suit.Herz, Rank.Dame);

    public override SonderkarteType Type => SonderkarteType.Genscherdamen;
    public override CardType TriggeringCard => HerzDame;

    public override bool AreConditionsMet(GameState state) =>
        !IsActive(state, SonderkarteType.Genscherdamen)
        && !IsWindowClosed(state, SonderkarteType.Genscherdamen)
        && OriginallyHeldBoth(state, HerzDame);

    protected override GameStateModification? ExtraEffects(
        GameState state,
        ISonderkarteInputProvider inputs
    ) => new SetGenscherPartnerModification(state.CurrentTurn, inputs.GetGenscherPartner());
}
