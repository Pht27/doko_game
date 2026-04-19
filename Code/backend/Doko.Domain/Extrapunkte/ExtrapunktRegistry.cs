using Doko.Domain.Reservations;
using Doko.Domain.Rules;

namespace Doko.Domain.Extrapunkte;

public static class ExtrapunktRegistry
{
    public static IReadOnlyList<IExtrapunkt> GetActive(
        RuleSet rules,
        IReservation? activeReservation
    )
    {
        if (activeReservation?.IsSolo == true)
            return [];

        var list = new List<IExtrapunkt>();
        if (rules.EnableDoppelkopf)
            list.Add(new DoppelkopfExtrapunkt());
        if (rules.EnableFuchsGefangen)
            list.Add(new FuchsGefangenExtrapunkt());
        if (rules.EnableKarlchen)
            list.Add(new KarlchenExtrapunkt());
        if (rules.EnableAgathe)
            list.Add(new AgatheExtrapunkt());
        if (rules.EnableFischauge)
            list.Add(new FischaugeExtrapunkt());
        if (rules.EnableGansGefangen)
            list.Add(new GansGefangenExtrapunkt());
        if (rules.EnableBlutbad)
            list.Add(new BlutbadExtrapunkt()); // Blutbad before Festmahl
        if (rules.EnableFestmahl)
            list.Add(new FestmahlExtrapunkt());
        if (rules.EnableKlabautermann)
            list.Add(new KlabautermannExtrapunkt());
        if (rules.EnableKaffeekranzchen)
            list.Add(new KaffeekranzExtrapunkt());
        return list;
    }
}
