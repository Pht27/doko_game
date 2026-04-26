using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Rules;

namespace Doko.Domain.Tricks;

public static class TrickWinnerRuleRegistry
{
    private static readonly BlutbadTrickWinnerRule BlutbadRule = new();
    private static readonly FestmahlTrickWinnerRule FestmahlRule = new();
    private static readonly MeutereiTrickWinnerRule MeutereiRule = new();

    /// <summary>
    /// Returns the effective trick winner after applying all active trick-winner override rules.
    /// Blutbad takes precedence over Festmahl; Meuterei is evaluated last.
    /// Disabled in all Soli (same exclusion as Extrapunkte).
    /// </summary>
    public static PlayerSeat GetEffectiveWinner(
        Trick trick,
        GameState state,
        PlayerSeat normalWinner
    )
    {
        if (state.ActiveReservation?.IsSolo == true)
            return normalWinner;

        if (state.Rules.EnableBlutbad)
        {
            var w = BlutbadRule.TryGetOverride(trick, state, normalWinner);
            if (w.HasValue)
                return w.Value;
        }

        if (state.Rules.EnableFestmahl)
        {
            var w = FestmahlRule.TryGetOverride(trick, state, normalWinner);
            if (w.HasValue)
                return w.Value;
        }

        if (state.Rules.EnableMeuterei)
        {
            var w = MeutereiRule.TryGetOverride(trick, state, normalWinner);
            if (w.HasValue)
                return w.Value;
        }

        return normalWinner;
    }
}
