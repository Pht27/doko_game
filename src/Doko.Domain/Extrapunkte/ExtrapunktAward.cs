using Doko.Domain.Players;

namespace Doko.Domain.Extrapunkte;

public record ExtrapunktAward(
    ExtrapunktType Type,
    PlayerId BenefittingPlayer,
    int Delta);
