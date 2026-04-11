Zumindest in dem AUswertungsscreen ist es so, dass die Extrapunkte nicht zum Spielwert dazu gezählt werden. Dass der Spielwert erstmal Spielwert bleibt ist okay, aber dann muss noch das Gesamtergebnis berechnet werden:

Für die Gewinnerpartei wird der Spielwert eingetragen (mal Solofaktor, falls nicht gleich 1) und dann plus alle Extrapunkte, die Spieler dieser Partei gemacht haben minus alle die Extrapunkte die die gegnerischen Spieler gemacht haben.

Das sollte am Ende zumindest in der Spielauswertung stehen und muss dann ja irgendwann auch mal so in die DB eingebaut werden.

---

## Implementierungsplan

### Analyse

Der aktuelle `GameScorer` addiert die Extrapunkte (als Netto-Offset) bereits zum `GameValue`, was dem Wunsch widerspricht, dass Spielwert und Extrapunkte getrennt bleiben. Außerdem fehlt der Solofaktor (×3 bei Soli) komplett.

**Formel laut TODO:**
`Gesamtergebnis = Spielwert × SoloFaktor + winnerExtra - loserExtra`

### Betroffene Dateien

**Backend:**
- `src/backend/Doko.Domain/Reservations/IReservation.cs` – `bool IsSolo` als Default-Interface-Methode (prüft ob Priority ≤ SchlankerMartin)
- `src/backend/Doko.Domain/Scoring/GameResult.cs` – Neue Felder: `SoloFactor`, `TotalScore`
- `src/backend/Doko.Domain/Scoring/GameScorer.cs` – Extrapunkte NICHT in `GameValue`, stattdessen `SoloFactor` + `TotalScore` berechnen
- `src/backend/Doko.Api/DTOs/Responses/GameResultDto.cs` – Neue Felder `SoloFactor`, `TotalScore`
- `src/backend/Doko.Api/Mapping/DtoMapper.cs` – Neue Felder mappen

**Frontend:**
- `src/frontend/src/types/api.ts` – TypeScript-Typen aktualisieren
- `src/frontend/src/translations.ts` – Übersetzungen für neue Labels
- `src/frontend/src/components/ResultScreen/ResultScreen.tsx` – Gesamtergebnis-Sektion anzeigen

**Tests:**
- `src/tests/Doko.Domain.Tests/Scoring/GameScorerTests.cs` – Existierende Tests bleiben gültig (keine verwendet Extrapunkte-Werte in GameValue); neue Tests für Solofaktor + TotalScore ergänzen

### Nicht-triviale Entscheidungen

1. **IsSolo per Interface-Default**: `IReservation.IsSolo` prüft `Priority <= ReservationPriority.SchlankerMartin` – alle Soli haben Priority 0–8, alle Nicht-Soli 9+.

2. **Extrapunkte raus aus GameValue**: Die "Extrapunkte"-Komponente wird aus `ValueComponents` entfernt. Die Extrapunkte sind bereits in der "Zusatzpunkte"-Sektion sichtbar; dort sollen sie bleiben. Der `GameValue` wird zur reinen Rechengröße (Gewonnen + Schwellenwerte + Ansagen).

3. **Feigheit**: Bei Feigheit wird winner gewechselt → `winnerExtra`/`loserExtra` müssen nach dem Flip neu berechnet werden. `TotalScore = 1 + extraPenalty + newNetExtra`.

4. **Gesamtergebnis-Sektion nur anzeigen wenn nötig**: Im Frontend erscheint die neue Sektion nur, wenn `SoloFactor > 1` oder `TotalScore != GameValue` (d.h. Extrapunkte vorhanden).
