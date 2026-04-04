using Doko.Domain.GameFlow;
using Doko.Domain.Tricks;

namespace Doko.Domain.Extrapunkte;

public interface IExtrapunkt
{
    ExtrapunktType Type { get; }
    IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state);
}
