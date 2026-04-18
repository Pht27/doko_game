using Doko.Domain.Cards;

namespace Doko.Console.Scenarios;

/// <summary>
/// Ready-made scenarios for manual testing of Sonderkarten and reservations.
/// All scenarios default to player 0 holding the special cards.
/// </summary>
public static class Scenarios
{
    private static CardType CT(Suit s, Rank r) => new(s, r);

    // ── Sonderkarten ─────────────────────────────────────────────────────────

    /// <summary>P0 holds both ♦ Aces → eligible for Schweinchen.</summary>
    public static ScenarioConfig Schweinchen(int player = 0) =>
        new()
        {
            Name = $"Schweinchen (P{player} holds both ♦ Aces)",
            PlayerRequiredCards = { [player] = [CT(Suit.Karo, Rank.Ass), CT(Suit.Karo, Rank.Ass)] },
        };

    /// <summary>P0 holds both ♠ Jacks → eligible for Heidmann.</summary>
    public static ScenarioConfig Heidmann(int player = 0) =>
        new()
        {
            Name = $"Heidmann (P{player} holds both ♠ Jacks)",
            PlayerRequiredCards = { [player] = [CT(Suit.Pik, Rank.Bube), CT(Suit.Pik, Rank.Bube)] },
        };

    /// <summary>P0 holds both ♦ Jacks → eligible for LinksGehangter (and RechtsGehangter on the second).</summary>
    public static ScenarioConfig Gehangter(int player = 0) =>
        new()
        {
            Name = $"Links-/Rechtsgehängter (P{player} holds both ♦ Jacks)",
            PlayerRequiredCards =
            {
                [player] = [CT(Suit.Karo, Rank.Bube), CT(Suit.Karo, Rank.Bube)],
            },
        };

    /// <summary>P0 holds both ♥ Queens → eligible for Genscherdamen.</summary>
    public static ScenarioConfig Genscherdamen(int player = 0) =>
        new()
        {
            Name = $"Genscherdamen (P{player} holds both ♥ Queens)",
            PlayerRequiredCards =
            {
                [player] = [CT(Suit.Herz, Rank.Dame), CT(Suit.Herz, Rank.Dame)],
            },
        };

    /// <summary>P0 holds both ♥ Jacks → eligible for Kemmerich (withdraw an announcement).</summary>
    public static ScenarioConfig Kemmerich(int player = 0) =>
        new()
        {
            Name = $"Kemmerich (P{player} holds both ♥ Jacks)",
            PlayerRequiredCards =
            {
                [player] = [CT(Suit.Herz, Rank.Bube), CT(Suit.Herz, Rank.Bube)],
            },
        };

    /// <summary>P0 holds both ♦ Aces AND both ♠ Jacks → eligible for Schweinchen + Heidmann.</summary>
    public static ScenarioConfig SchweinchenUndHeidmann(int player = 0) =>
        new()
        {
            Name = $"Schweinchen + Heidmann (P{player})",
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

    /// <summary>
    /// P0 holds both ♣ Queens → eligible for Hochzeit.
    /// (Solos are always eligible by ruleset; no special card requirement.)
    /// </summary>
    public static ScenarioConfig Hochzeit(int player = 0) =>
        new()
        {
            Name = $"Hochzeit (P{player} holds both ♣ Queens)",
            PlayerRequiredCards =
            {
                [player] = [CT(Suit.Kreuz, Rank.Dame), CT(Suit.Kreuz, Rank.Dame)],
            },
        };

    /// <summary>
    /// P0 gets a pure Fehl hand (0 trumps) → eligible for Armut.
    /// Trumps excluded: Damen, Buben, ♥10 (Dulle), all ♦ except ♦ Ace.
    /// </summary>
    public static ScenarioConfig Armut(int player = 0) =>
        new()
        {
            Name = $"Armut (P{player} gets zero trumps)",
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

    // ── Combined / stress ────────────────────────────────────────────────────

    /// <summary>
    /// P0 has Schweinchen + Genscherdamen + Heidmann.
    /// All three trigger on different cards so they can all fire in one game.
    /// </summary>
    public static ScenarioConfig ManyBothPairs(int player = 0) =>
        new()
        {
            Name = $"Schweinchen + Genscherdamen + Heidmann (P{player})",
            PlayerRequiredCards =
            {
                [player] =
                [
                    CT(Suit.Karo, Rank.Ass),
                    CT(Suit.Karo, Rank.Ass), // Schweinchen
                    CT(Suit.Herz, Rank.Dame),
                    CT(Suit.Herz, Rank.Dame), // Genscherdamen
                    CT(Suit.Pik, Rank.Bube),
                    CT(Suit.Pik, Rank.Bube), // Heidmann
                ],
            },
        };

    // ── All scenarios listed for the menu ────────────────────────────────────

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
            ManyBothPairs(),
        ];
}
