using Doko.Domain.Cards;

namespace Doko.Application.Scenarios;

/// <summary>
/// Describes how to pre-arrange a deck for manual testing.
/// For each player (0–3) you can specify a list of CardTypes they must receive.
/// Duplicates mean "give me both physical copies of that card type".
/// </summary>
public sealed class ScenarioConfig
{
    public string Name { get; init; } = "Custom";

    /// <summary>Player index → ordered list of required CardTypes (duplicates allowed).</summary>
    public Dictionary<int, List<CardType>> PlayerRequiredCards { get; init; } = [];
}
