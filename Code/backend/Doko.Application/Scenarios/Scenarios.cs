using Doko.Domain.Cards;

namespace Doko.Application.Scenarios;

/// <summary>
/// Ready-made scenarios for testing Sonderkarten and reservations.
/// All scenarios default to player 0 holding the special cards.
/// </summary>
public static class Scenarios
{
    private static CardType CT(Suit s, Rank r) => new(s, r);

    // ── Sonderkarten ─────────────────────────────────────────────────────────

    public static ScenarioConfig Schweinchen(int player = 0) =>
        new()
        {
            Name = "Schweinchen",
            PlayerRequiredCards = { [player] = [CT(Suit.Karo, Rank.Ass), CT(Suit.Karo, Rank.Ass)] },
        };

    public static ScenarioConfig Heidmann(int player = 0) =>
        new()
        {
            Name = "Heidmann",
            PlayerRequiredCards = { [player] = [CT(Suit.Pik, Rank.Bube), CT(Suit.Pik, Rank.Bube)] },
        };

    public static ScenarioConfig Gehangter(int player = 0) =>
        new()
        {
            Name = "Gehängter",
            PlayerRequiredCards =
            {
                [player] = [CT(Suit.Karo, Rank.Bube), CT(Suit.Karo, Rank.Bube)],
            },
        };

    public static ScenarioConfig Genscherdamen(int player = 0) =>
        new()
        {
            Name = "Genscherdamen",
            PlayerRequiredCards =
            {
                [player] = [CT(Suit.Herz, Rank.Dame), CT(Suit.Herz, Rank.Dame)],
            },
        };

    public static ScenarioConfig Kemmerich(int player = 0) =>
        new()
        {
            Name = "Kemmerich",
            PlayerRequiredCards =
            {
                [player] = [CT(Suit.Herz, Rank.Bube), CT(Suit.Herz, Rank.Bube)],
            },
        };

    public static ScenarioConfig SchweinchenUndHeidmann(int player = 0) =>
        new()
        {
            Name = "Schweinchen + Heidmann",
            PlayerRequiredCards =
            {
                [player] =
                [
                    CT(Suit.Karo, Rank.Ass),
                    CT(Suit.Karo, Rank.Ass),
                    CT(Suit.Pik, Rank.Bube),
                    CT(Suit.Pik, Rank.Bube),
                ],
            },
        };

    // ── Reservations ─────────────────────────────────────────────────────────

    public static ScenarioConfig Hochzeit(int player = 0) =>
        new()
        {
            Name = "Hochzeit",
            PlayerRequiredCards =
            {
                [player] = [CT(Suit.Kreuz, Rank.Dame), CT(Suit.Kreuz, Rank.Dame)],
            },
        };

    public static ScenarioConfig Armut(int player = 0) =>
        new()
        {
            Name = "Armut",
            PlayerRequiredCards =
            {
                [player] =
                [
                    CT(Suit.Kreuz, Rank.Ass),
                    CT(Suit.Kreuz, Rank.Ass),
                    CT(Suit.Kreuz, Rank.Koenig),
                    CT(Suit.Kreuz, Rank.Koenig),
                    CT(Suit.Pik, Rank.Ass),
                    CT(Suit.Pik, Rank.Ass),
                    CT(Suit.Pik, Rank.Koenig),
                    CT(Suit.Pik, Rank.Koenig),
                    CT(Suit.Herz, Rank.Ass),
                    CT(Suit.Herz, Rank.Ass),
                    CT(Suit.Herz, Rank.Koenig),
                    CT(Suit.Herz, Rank.Koenig),
                ],
            },
        };

    /// <summary>P0 holds both ♠ Queens + both ♠ Kings → triggers KontraSolo silent game mode.</summary>
    public static ScenarioConfig Kontrasolo(int player = 0) =>
        new()
        {
            Name = "Kontrasolo",
            PlayerRequiredCards =
            {
                [player] =
                [
                    CT(Suit.Pik, Rank.Dame),
                    CT(Suit.Pik, Rank.Dame),
                    CT(Suit.Pik, Rank.Koenig),
                    CT(Suit.Pik, Rank.Koenig),
                ],
            },
        };

    // ── Combined ─────────────────────────────────────────────────────────────

    public static ScenarioConfig ManyBothPairs(int player = 0) =>
        new()
        {
            Name = "Schweinchen + Genscherdamen + Heidmann",
            PlayerRequiredCards =
            {
                [player] =
                [
                    CT(Suit.Karo, Rank.Ass),
                    CT(Suit.Karo, Rank.Ass),
                    CT(Suit.Herz, Rank.Dame),
                    CT(Suit.Herz, Rank.Dame),
                    CT(Suit.Pik, Rank.Bube),
                    CT(Suit.Pik, Rank.Bube),
                ],
            },
        };

    public static ScenarioConfig[] All =>
        [
            Schweinchen(),
            Heidmann(),
            Gehangter(),
            Genscherdamen(),
            Kemmerich(),
            SchweinchenUndHeidmann(),
            Hochzeit(),
            Armut(),
            Kontrasolo(),
            ManyBothPairs(),
        ];
}
