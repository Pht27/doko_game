using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

/// <summary>
/// "Animal" cards used by Festmahl and Blutbad:
/// Fischaugen (♦9 after first trump played) + the Schweinchen family (♦A/♦10/♦K when active).
/// </summary>
internal enum AnimalKind { Fischauge, Schweinchen, Superschweinchen, Hyperschweinchen }

internal static class AnimalHelpers
{
    private static readonly CardType KaroNeun   = new(Suit.Karo, Rank.Neun);
    private static readonly CardType KaroAss    = new(Suit.Karo, Rank.Ass);
    private static readonly CardType KaroZehn   = new(Suit.Karo, Rank.Zehn);
    private static readonly CardType KaroKoenig = new(Suit.Karo, Rank.Koenig);

    internal static AnimalKind? GetAnimalKind(TrickCard tc, GameState state)
    {
        var type = tc.Card.Type;
        if (type == KaroNeun   && FischaugeActive(state))                                                     return AnimalKind.Fischauge;
        if (type == KaroAss    && state.ActiveSonderkarten.Contains(SonderkarteType.Schweinchen))             return AnimalKind.Schweinchen;
        if (type == KaroZehn   && state.ActiveSonderkarten.Contains(SonderkarteType.Superschweinchen))        return AnimalKind.Superschweinchen;
        if (type == KaroKoenig && state.ActiveSonderkarten.Contains(SonderkarteType.Hyperschweinchen))        return AnimalKind.Hyperschweinchen;
        return null;
    }

    /// <summary>True once any trump card has been played in a completed trick.</summary>
    internal static bool FischaugeActive(GameState state)
        => state.CompletedTricks.Any(t => t.Cards.Any(tc => state.TrumpEvaluator.IsTrump(tc.Card.Type)));

    internal static List<(TrickCard Card, AnimalKind Kind)> GetAnimals(Trick trick, GameState state)
        => trick.Cards
            .Select(tc => (Card: tc, Kind: GetAnimalKind(tc, state)))
            .Where(x => x.Kind.HasValue)
            .Select(x => (Card: x.Card, Kind: x.Kind!.Value))
            .ToList();
}
