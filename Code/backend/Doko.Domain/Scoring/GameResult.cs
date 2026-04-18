using Doko.Domain.Announcements;
using Doko.Domain.Extrapunkte;
using Doko.Domain.Parties;

namespace Doko.Domain.Scoring;

public record GameValueComponent(string Label, int Value);

public record AnnouncementRecord(Party Party, AnnouncementType AnnouncementType);

public record GameResult(
    Party Winner,
    int ReAugen,
    int KontraAugen,
    int ReStiche,
    int KontraStiche,
    int GameValue,
    IReadOnlyList<ExtrapunktAward> AllAwards,
    bool Feigheit,
    IReadOnlyList<GameValueComponent> ValueComponents,
    int SoloFactor,
    int TotalScore,
    IReadOnlyList<AnnouncementRecord> AnnouncementRecords
);
