using Doko.Domain.Extrapunkte;
using Doko.Domain.Parties;

namespace Doko.Domain.Scoring;

public record GameResult(
    Party Winner,
    int RePoints,
    int KontraPoints,
    int GameValue,
    IReadOnlyList<ExtrapunktAward> AllAwards,
    bool Feigheit);
