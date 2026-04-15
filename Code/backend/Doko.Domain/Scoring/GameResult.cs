using Doko.Domain.Extrapunkte;
using Doko.Domain.Parties;

namespace Doko.Domain.Scoring;

public record GameValueComponent(string Label, int Value);

public record GameResult(
    Party Winner,
    int ReAugen,
    int KontraAugen,
    int GameValue,
    IReadOnlyList<ExtrapunktAward> AllAwards,
    bool Feigheit,
    IReadOnlyList<GameValueComponent> ValueComponents,
    int SoloFactor,
    int TotalScore
);
