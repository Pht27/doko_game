using Doko.Domain.Cards;
using Doko.Domain.GameFlow;

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

    public override bool AreConditionsMet(GameState state)
        => !IsActive(state, SonderkarteType.LinksGehangter)
        && OriginallyHeldBoth(state, KaroBube);

    public override GameStateModification? Apply(GameState state) => new ReverseDirectionModification();
}
