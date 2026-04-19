using Doko.Domain.Players;

namespace Doko.Domain.Extrapunkte;

public record ExtrapunktAward(ExtrapunktType Type, PlayerSeat BenefittingPlayer, int Delta);
