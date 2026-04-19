using Doko.Domain.Extrapunkte;
using Doko.Domain.Players;
using Doko.Domain.Tricks;

namespace Doko.Domain.Scoring;

public record TrickResult(Trick Trick, PlayerSeat Winner, IReadOnlyList<ExtrapunktAward> Awards);
