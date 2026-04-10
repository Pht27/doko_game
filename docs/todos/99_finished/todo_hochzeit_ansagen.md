Wenn bei Hohczeiten der Findungsstich nicht der erste ist, kann danach nicht mehr angesagt werden? checken

es sollte so sein: man kann Ansagen bis vor der zweiten Karte im Stich nach dem Findungsstich. Davor nicht. Bei Ansagen wird es wie üblich um einen Stich verlängert

## Analyse

**Bug bestätigt:** Die Deadline für Ansagen wird immer relativ zum Spielstart berechnet
(`deadline = 5 + 4 * totalAnnouncements`). Ist der Findungsstich z.B. der 5. Stich (Trick 4),
sind bereits 20 Karten gespielt — die Deadline von 5 ist längst überschritten.

Außerdem: Der Hochzeitspieler kann aktuell vor dem Findungsstich ansagen (seine Partei ist
sofort bekannt), was laut Regel ebenfalls verboten ist.

## Implementierungsplan

### Betroffene Dateien

| Datei | Änderung |
|---|---|
| `IPartyResolver.cs` | Neue Methode `AnnouncementBaseDeadline(GameState)` hinzufügen |
| `SoloPartyResolver.cs` | Implementierung: return 5 |
| `NormalPartyResolver.cs` | Implementierung: return 5 |
| `ArmutPartyResolver.cs` | Implementierung: return 5 |
| `GenscherPartyResolver.cs` | Implementierung: return 5 |
| `HochzeitPartyResolver.cs` | Implementierung: null vor Findungsstich; K*4+5 danach |
| `AnnouncementRules.cs` | Timing-Check auf `AnnouncementBaseDeadline` umstellen |
| `AnnouncementRulesTests.cs` | Hochzeit-spezifische Timing-Tests hinzufügen |

### Logik

**Neue Interface-Methode:**
```csharp
/// <summary>
/// Returns the base deadline (total cards played, exclusive upper bound) for announcements.
/// Returns null if announcements are not yet allowed (e.g. Hochzeit before Findungsstich).
/// </summary>
int? AnnouncementBaseDeadline(GameState state);
```

**Hochzeit-Formel:** Findungsstich an Index K → Deadline = K*4 + 5
- K=0 (erster Stich): Deadline 5 (entspricht Normalspielebene)
- K=1: Deadline 9 (bis vor 2. Karte des 3. Stichs)
- K=2: Deadline 13 usw.

**Timing-Check in `AnnouncementRules.CanAnnounce`:**
```csharp
var baseDeadline = state.PartyResolver.AnnouncementBaseDeadline(state);
if (baseDeadline is null)
    return false;
int deadline = baseDeadline.Value + 4 * totalAnnouncements;
if (totalCardsPlayed >= deadline)
    return false;
```

### Trade-offs
- Interface-Erweiterung: alle 5 Resolver müssen die Methode implementieren, aber die
  Default-Implementierung ist trivial (return 5).
- Verhalten für den Hochzeitsspieler selbst: er kann nun ebenfalls nicht mehr vor dem
  Findungsstich ansagen (korrekt laut Regel).
