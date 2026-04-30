using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Sonderkarten;

namespace Doko.Domain.Tricks;

/// <summary>
/// "Animal" cards used by Festmahl and Blutbad:
/// Fuchs/Schweinchen (♦A), Fischauge (♦9 after first trump), Superschweinchen (♦10), Hyperschweinchen (♦K).
/// ♦A is always an animal — as Fuchs when Schweinchen is inactive, as Schweinchen when active.
/// </summary>
internal enum AnimalKind
{
    Fuchs,
    Fischauge,
    Schweinchen,
    Superschweinchen,
    Hyperschweinchen,
}

internal static class AnimalHelpers
{
    private static readonly CardType KaroNeun = new(Suit.Karo, Rank.Neun);
    private static readonly CardType KaroAss = new(Suit.Karo, Rank.Ass);
    private static readonly CardType KaroZehn = new(Suit.Karo, Rank.Zehn);
    private static readonly CardType KaroKoenig = new(Suit.Karo, Rank.Koenig);

    internal static AnimalKind? GetAnimalKind(TrickCard tc, PlayingState state)
    {
        var type = tc.Card.Type;
        if (type == KaroNeun && FischaugeActive(state))
            return AnimalKind.Fischauge;
        if (type == KaroAss)
            return state.ActiveSonderkarten.Contains(SonderkarteType.Schweinchen)
                ? AnimalKind.Schweinchen
                : AnimalKind.Fuchs;
        if (type == KaroZehn && state.ActiveSonderkarten.Contains(SonderkarteType.Superschweinchen))
            return AnimalKind.Superschweinchen;
        if (
            type == KaroKoenig
            && state.ActiveSonderkarten.Contains(SonderkarteType.Hyperschweinchen)
        )
            return AnimalKind.Hyperschweinchen;
        return null;
    }

    /// <summary>True once any trump card has been played in a completed trick.</summary>
    internal static bool FischaugeActive(PlayingState state) =>
        state.CompletedTricks.Any(t =>
            t.Cards.Any(tc => state.TrumpEvaluator.IsTrump(tc.Card.Type))
        );

    /// <summary>True once any trump card has been played in a completed trick. Works with ScoringState.</summary>
    internal static bool FischaugeActive(ScoringState state) =>
        state.CompletedTricks.Any(t =>
            t.Cards.Any(tc => state.TrumpEvaluator.IsTrump(tc.Card.Type))
        );

    /// <summary>
    /// Overload for callers that hold a base <see cref="GameState"/> (e.g. extrapunkt evaluation
    /// in the scorer). Returns false for state types that carry no completed tricks.
    /// </summary>
    internal static bool FischaugeActive(GameState state) =>
        state switch
        {
            PlayingState p => FischaugeActive(p),
            ScoringState s => FischaugeActive(s),
            _ => false,
        };

    internal static List<(TrickCard Card, AnimalKind Kind)> GetAnimals(
        Trick trick,
        PlayingState state
    ) =>
        trick
            .Cards.Select(tc => (Card: tc, Kind: GetAnimalKind(tc, state)))
            .Where(x => x.Kind.HasValue)
            .Select(x => (Card: x.Card, Kind: x.Kind!.Value))
            .ToList();
}
