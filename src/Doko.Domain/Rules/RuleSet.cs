namespace Doko.Domain.Rules;

public record RuleSet
{
    // Deck
    public bool PlayWithNines { get; init; }

    // Game modes
    public bool AllowFarbsoli { get; init; }
    public bool AllowDamensolo { get; init; }
    public bool AllowBubensolo { get; init; }
    public bool AllowFleischloses { get; init; }
    public bool AllowNullo { get; init; }
    public bool AllowKnochenloses { get; init; }
    public bool AllowSchlankerMartin { get; init; }
    public bool AllowStilleSolo { get; init; }
    public bool AllowArmut { get; init; }
    public bool AllowHochzeit { get; init; }
    public bool AllowSchmeissen { get; init; }

    // Dulle tie-break
    public DulleRule DulleRule { get; init; }

    // Sonderkarten
    public bool EnableSchweinchen { get; init; }
    public bool EnableSuperschweinchen { get; init; }
    public bool EnableHyperschweinchen { get; init; }
    public bool EnableLinksGehangter { get; init; }
    public bool EnableRechtsGehangter { get; init; }
    public bool EnableGenscherdamen { get; init; }
    public bool EnableGegengenscherdamen { get; init; }
    public bool EnableHeidmann { get; init; }
    public bool EnableHeidfrau { get; init; }
    public bool EnableKemmerich { get; init; }
    public bool EnableSchatz { get; init; }

    // Announcements
    public bool AllowAnnouncements { get; init; }
    public bool EnforcePflichtansage { get; init; }
    public bool EnforceFeigheit { get; init; }

    // Extrapunkte
    public bool EnableDoppelkopf { get; init; }
    public bool EnableFuchsGefangen { get; init; }
    public bool EnableKarlchen { get; init; }
    public bool EnableAgathe { get; init; }
    public bool EnableFischauge { get; init; }
    public bool EnableGansGefangen { get; init; }
    public bool EnableFestmahl { get; init; }
    public bool EnableBlutbad { get; init; }
    public bool EnableKlabautermann { get; init; }
    public bool EnableKaffeekranzchen { get; init; }

    /// <summary>Standard Koppeldopf rules with all optional features enabled.</summary>
    public static RuleSet Default() => new()
    {
        PlayWithNines          = true,
        AllowFarbsoli          = true,
        AllowDamensolo         = true,
        AllowBubensolo         = true,
        AllowFleischloses      = true,
        AllowKnochenloses      = true,
        AllowSchlankerMartin   = true,
        AllowStilleSolo        = true,
        AllowArmut             = true,
        AllowHochzeit          = true,
        AllowSchmeissen        = true,
        DulleRule              = DulleRule.SecondBeatsFirst,
        EnableSchweinchen      = true,
        EnableSuperschweinchen = true,
        EnableHyperschweinchen = true,
        EnableLinksGehangter   = true,
        EnableRechtsGehangter  = true,
        EnableGenscherdamen    = true,
        EnableGegengenscherdamen = true,
        EnableHeidmann         = true,
        EnableHeidfrau         = true,
        EnableKemmerich        = true,
        EnableSchatz           = true,
        AllowAnnouncements     = true,
        EnforcePflichtansage   = true,
        EnforceFeigheit        = true,
        EnableDoppelkopf       = true,
        EnableFuchsGefangen    = true,
        EnableKarlchen         = true,
        EnableAgathe           = true,
        EnableFischauge        = true,
        EnableGansGefangen     = true,
        EnableFestmahl         = true,
        EnableBlutbad          = true,
        EnableKlabautermann    = true,
        EnableKaffeekranzchen  = true,
    };

    /// <summary>Minimal rules with all optional features disabled.</summary>
    public static RuleSet Minimal() => new()
    {
        DulleRule = DulleRule.SecondBeatsFirst,
        // All bool properties default to false — no optional features enabled
    };
}
