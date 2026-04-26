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

        bool isArmut = activeReservation is ArmutReservation;

        var list = new List<IExtrapunkt>();
        if (rules.EnableDoppelkopf)
            list.Add(new DoppelkopfExtrapunkt());
        if (rules.EnableFuchsGefangen)
            list.Add(new FuchsGefangenExtrapunkt());
        if (rules.EnableKarlchen && !isArmut)
            list.Add(new KarlchenExtrapunkt());
        if (rules.EnableAgathe && !isArmut)
            list.Add(new AgatheExtrapunkt());
        if (rules.EnableFischauge && !isArmut)
            list.Add(new FischaugeExtrapunkt());
        if (rules.EnableGansGefangen)
            list.Add(new GansGefangenExtrapunkt());
        if (rules.EnableKlabautermann)
            list.Add(new KlabautermannExtrapunkt());
        if (rules.EnableKaffeekranzchen)
            list.Add(new KaffeekranzExtrapunkt());
        return list;
    }
}
