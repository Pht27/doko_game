using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Rules;

namespace Doko.Domain.Sonderkarten;

public static class SonderkarteRegistry
{
    /// <summary>All sonderkarten enabled by the ruleset, regardless of game state or hand.</summary>
    public static IReadOnlyList<ISonderkarte> GetEnabled(RuleSet rules)
    {
        var list = new List<ISonderkarte>();
        if (rules.EnableSchweinchen)
            list.Add(new SchweinSonderkarte());
        if (rules.EnableSuperschweinchen)
            list.Add(new SuperschweinchenSonderkarte());
        if (rules.EnableHyperschweinchen)
            list.Add(new HyperschweinchenSonderkarte());
        if (rules.EnableLinksGehangter)
            list.Add(new LinksGehangterSonderkarte());
        if (rules.EnableRechtsGehangter)
            list.Add(new RechtsGehangterSonderkarte());
        if (rules.EnableGenscherdamen)
            list.Add(new GenscherdamenSonderkarte());
        if (rules.EnableGegengenscherdamen)
            list.Add(new GegengenscherdamenSonderkarte());
        if (rules.EnableHeidmann)
            list.Add(new HeidmannSonderkarte());
        if (rules.EnableHeidfrau)
            list.Add(new HeidfrauSonderkarte());
        if (rules.EnableKemmerich)
            list.Add(new KemmerichSonderkarte());
        return list;
    }

    /// <summary>
    /// Returns the sonderkarten the player may claim when playing <paramref name="playedCard"/>.
    /// Called before the card is removed from the player's hand in <paramref name="state"/>.
    /// </summary>
    public static IReadOnlyList<ISonderkarte> GetEligibleForCard(
        Card playedCard,
        GameState state,
        RuleSet rules
    ) =>
        GetEnabled(rules)
            .Where(s => s.TriggeringCard == playedCard.Type && s.AreConditionsMet(state))
            .ToList();
}
