using Doko.Domain.Cards;
using Doko.Domain.GameFlow;

namespace Doko.Domain.Sonderkarten;

/// <summary>
/// Schatz (WIP): player originally held both ♥ Nines → when playing a ♥ Nine, may announce
/// "Schatz" — the Augen value of the ♥ 10s (Dullen) is transferred to the ♥ 9s for scoring.
/// Design is preliminary; timing rules and further interactions are pending.
/// </summary>
public sealed class SchatzSonderkarte : SonderkarteBase
{
    private static readonly CardType HerzNeun = new(Suit.Herz, Rank.Neun);
    private static readonly CardType HerzZehn = new(Suit.Herz, Rank.Zehn);

    public override SonderkarteType Type => SonderkarteType.Schatz;
    public override CardType TriggeringCard => HerzNeun;

    public override bool AreConditionsMet(GameState state) =>
        !IsActive(state, SonderkarteType.Schatz) && OriginallyHeldBoth(state, HerzNeun);

    protected override GameStateModification? ExtraEffects(GameState state, ISonderkarteInputProvider inputs) =>
        new TransferCardPointsModification(HerzZehn, HerzNeun);
}
