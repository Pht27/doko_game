using Doko.Domain.Cards;

namespace Doko.Console.Scenarios;

/// <summary>
/// Describes how to pre-arrange a deck for manual testing.
/// For each player (0–3) you can specify a list of CardTypes they must receive.
/// Duplicates mean "give me both physical copies of that card type" (e.g. both ♦ Aces for Schweinchen).
/// Invalid configs (e.g. asking for 3 copies of a card that only exists twice) are the caller's problem.
/// </summary>
public sealed class ScenarioConfig
{
    public string Name { get; init; } = "Custom";

    /// <summary>Player index → ordered list of required CardTypes (duplicates allowed).</summary>
    public Dictionary<int, List<CardType>> PlayerRequiredCards { get; init; } = [];
}
