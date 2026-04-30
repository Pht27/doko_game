using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Rules;

namespace Doko.Domain.Tricks;

public static class TrickWinnerRuleRegistry
{
    private static readonly ITrickWinnerRule[] Rules =
    [
        new BlutbadTrickWinnerRule(),
        new FestmahlTrickWinnerRule(),
        new MeutereiTrickWinnerRule(),
    ];

    /// <summary>
    /// Returns the effective trick winner after applying all active trick-winner override rules.
    /// Blutbad takes precedence over Festmahl; Meuterei is evaluated last.
    /// Disabled in all Soli (same exclusion as Extrapunkte).
    /// </summary>
    public static PlayerSeat GetEffectiveWinner(
        Trick trick,
        PlayingState state,
        PlayerSeat normalWinner
    )
    {
        if (state.ActiveReservation?.IsSolo == true)
            return normalWinner;

        foreach (var rule in Rules.Where(r => r.IsEnabledBy(state.Rules)))
        {
            var w = rule.TryGetOverride(trick, state, normalWinner);
            if (w.HasValue)
                return w.Value;
        }

        return normalWinner;
    }
}
