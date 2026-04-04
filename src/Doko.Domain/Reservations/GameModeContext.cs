using Doko.Domain.Parties;
using Doko.Domain.Trump;

namespace Doko.Domain.Reservations;

public record GameModeContext(
    ITrumpEvaluator TrumpEvaluator,
    IPartyResolver PartyResolver);
