using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

public interface IExtrapunkt
{
    ExtrapunktType Type { get; }

    /// <summary>
    /// When true, the scorer re-evaluates this extrapunkt for all tricks using the final
    /// game state instead of the per-trick stored awards. Required for any extrapunkt that
    /// checks party membership, so Genscher team changes are reflected correctly.
    /// </summary>
    bool UsesFinalPartyState => false;

    /// <summary>
    /// Evaluates this extrapunkt for the given completed trick.
    /// <paramref name="effectiveTrickWinner"/> is the actual trick winner after all
    /// <see cref="ITrickWinnerRule"/> overrides have been applied.
    /// </summary>
    IReadOnlyList<ExtrapunktAward> Evaluate(
        Trick completedTrick,
        GameState state,
        PlayerSeat effectiveTrickWinner
    );
}
