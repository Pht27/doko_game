# Fischauge: Karte verdeckt anzeigen

## Beschreibung

Wenn eine ♦9 (Karo Neun / Fischauge) gespielt wird, soll sie im Stichbereich verdeckt angezeigt werden — vorausgesetzt, beide Bedingungen sind erfüllt:

1. Fischauge ist aktiv (d.h. in einem bereits abgeschlossenen Stich wurde mindestens eine Trumpfkarte gespielt)
2. Die ♦9 ist **nicht** die erste Karte im aktuellen Stich
3. Alle bisher gespielten Karten im aktuellen Stich sind Fehlkarten (kein Trumpf)

Hintergrund: Da erst am Ende des Stichs feststeht, ob die ♦9 tatsächlich gewinnt (Fischauge), soll die Karte während des laufenden Stichs verdeckt liegen, um Spannung zu erzeugen.

---

## Implementierungsplan

### Betroffene Dateien

**Backend:**
- `TrickCardSummary.cs` — `FaceDown`-Flag ergänzen
- `GameQueryService.cs` — Face-down-Logik für aktuellen Stich berechnen
- `TrickSummaryDto.cs` (API) — `FaceDown` in `TrickCardDto` ergänzen
- `DtoMapper.cs` — `FaceDown` durchmappen

**Frontend:**
- `api.ts` — `faceDown: boolean` zu `TrickCardDto` ergänzen
- `TrickArea.tsx` — Per-Karte `faceDown` verwenden (zusätzlich zu Animation-basiertem `isFlipped`)

### Logik (Backend, `GameQueryService`)

Nur für den **aktuellen, noch laufenden Stich** (nicht für abgeschlossene Stiche):

```csharp
bool fischaugeActive = state.CompletedTricks.Any(t =>
    t.Cards.Any(tc => state.TrumpEvaluator.IsTrump(tc.Card.Type)));

var karoNeun = new CardType(Suit.Karo, Rank.Neun);

// Für jede Karte im aktuellen Stich:
bool faceDown =
    fischaugeActive
    && tc.Card.Type == karoNeun
    && index > 0
    && trick.Cards.Take(index).All(prev => !state.TrumpEvaluator.IsTrump(prev.Card.Type));
```

### Entscheidungen / Trade-offs

- Face-down nur für den **laufenden Stich** — abgeschlossene Stiche werden immer aufgedeckt angezeigt.
- Die Logik wird im Backend berechnet, da der Frontend die Trumpf-Auswertung je nach Spielmodus nicht kennt.
- `AnimalHelpers.FischaugeActive` ist `internal` → Bedingung wird in `GameQueryService` inline repliziert (gleiche Logik, keine neue public API nötig).
- Im Frontend: `faceDown={trickCard.faceDown || isFlipped}` — das Backend-Flag und die Animations-Phasen bleiben unabhängig.
