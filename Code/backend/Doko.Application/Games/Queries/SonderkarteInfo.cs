using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.Queries;

/// <summary>
/// Presentation-layer metadata for an eligible sonderkarte.
/// Provides the UI with what it needs to prompt the player — the type to send back in the command,
/// plus a human-readable name and short description of the effect.
/// </summary>
public record SonderkarteInfo(SonderkarteType Type, string Name, string Description)
{
    /// <summary>Returns <see cref="SonderkarteInfo"/> for every known type.</summary>
    public static readonly IReadOnlyDictionary<SonderkarteType, SonderkarteInfo> All =
        new Dictionary<SonderkarteType, SonderkarteInfo>
        {
            [SonderkarteType.Schweinchen] = new(
                SonderkarteType.Schweinchen,
                "Schweinchen",
                "You originally held both ♦ Aces — they become the two highest trumps, above the Dullen."
            ),

            [SonderkarteType.Superschweinchen] = new(
                SonderkarteType.Superschweinchen,
                "Superschweinchen",
                "Schweinchen is active and you originally held both ♦ 10s — your ♦ 10 becomes the single highest trump, above the Schweinchen."
            ),

            [SonderkarteType.Hyperschweinchen] = new(
                SonderkarteType.Hyperschweinchen,
                "Hyperschweinchen",
                "Superschweinchen is active and you originally held both ♦ Kings — your ♦ King ranks above even the Superschweinchen."
            ),

            [SonderkarteType.LinksGehangter] = new(
                SonderkarteType.LinksGehangter,
                "Linksdrehender Gehängter",
                "You originally held both ♦ Jacks — playing the first one reverses the play direction."
            ),

            [SonderkarteType.RechtsGehangter] = new(
                SonderkarteType.RechtsGehangter,
                "Rechtsdrehender Gehängter",
                "LinksGehangter is active — playing your second ♦ Jack reverses the play direction back again."
            ),

            [SonderkarteType.Genscherdamen] = new(
                SonderkarteType.Genscherdamen,
                "Genscherdamen",
                "You originally held both ♥ Queens — you may declare a new partner, making you and them the Re party."
            ),

            [SonderkarteType.Gegengenscherdamen] = new(
                SonderkarteType.Gegengenscherdamen,
                "Gegengenscherdamen",
                "Genscherdamen is active — you may counter-declare a new partner of your own."
            ),

            [SonderkarteType.Heidmann] = new(
                SonderkarteType.Heidmann,
                "Heidmann",
                "You originally held both ♠ Jacks — Jacks now rank above Queens for the rest of the game."
            ),

            [SonderkarteType.Heidfrau] = new(
                SonderkarteType.Heidfrau,
                "Heidfrau",
                "Heidmann is active and you originally held both ♠ Queens — you may revert Heidmann, restoring the normal Queens-above-Jacks order."
            ),

            [SonderkarteType.Kemmerich] = new(
                SonderkarteType.Kemmerich,
                "Kemmerich",
                "You originally held both ♥ Jacks — you may withdraw one announcement your party has made."
            ),

            [SonderkarteType.Schatz] = new(
                SonderkarteType.Schatz,
                "Schatz",
                "You originally held both ♥ 9s — the point value of ♥ 10s (Dullen) transfers to your ♥ 9s for scoring."
            ),
        };

    public static SonderkarteInfo For(SonderkarteType type) => All[type];
}
