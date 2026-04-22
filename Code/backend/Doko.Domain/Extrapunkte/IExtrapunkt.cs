using Doko.Domain.GameFlow;
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

    IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state);
}
