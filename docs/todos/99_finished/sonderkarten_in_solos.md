lass uns irgendwie eine kohärente logik einbauen, die speichert bei welchem vorbehalt was für sonderkarten oder extrapuntke existieren. die genauen regeln sind

1. in keinem solo außer schlanker martin sind extrapunkte oder sonderkarten aktiv
2. im schlanken martin sind keine extrapunkte aktiv, aber alle sonderkarten außer genschern
2. bei der armut gibt es keine sonderkarten. extrapunkte sind alle normal aktiv
3. bei der hochzeit sind sonderkarten und extrapunkte wie im normalspiel

---

## Implementierungsplan

### Regeln zusammengefasst

| Vorbehalt              | Sonderkarten       | Extrapunkte |
|------------------------|--------------------|-------------|
| Solo (außer Sch.M.)    | keine              | keine       |
| Schlanker Martin       | alle außer Genscher| keine       |
| Armut                  | keine              | normal      |
| Hochzeit               | normal             | normal      |
| Normalspiel            | normal             | normal      |

### Betroffene Dateien

1. **`SonderkarteRegistry.cs`** (`Doko.Domain/Sonderkarten/`)
   - `GetEligibleForCard` hat bereits `GameState` → dort Filterlogik anhand `state.ActiveReservation` ergänzen.

2. **`ExtrapunktRegistry.cs`** (`Doko.Domain/Extrapunkte/`)
   - `GetActive(RuleSet)` → Parameter `IReservation? activeReservation` hinzufügen.
   - Wenn `activeReservation?.IsSolo == true` → leere Liste zurückgeben.

3. **`PlayCardHandler.cs`** (`Doko.Application/Games/Handlers/`)
   - Aufruf `ExtrapunktRegistry.GetActive(state.Rules)` → `ExtrapunktRegistry.GetActive(state.Rules, state.ActiveReservation)`.

### Filterlogik in SonderkarteRegistry.GetEligibleForCard

```
var reservation = state.ActiveReservation;

// Solos (außer SchlankerMartin) und Armut: keine Sonderkarten
bool noSonderkarten =
    (reservation?.IsSolo == true && reservation.Priority != ReservationPriority.SchlankerMartin)
    || reservation?.Priority == ReservationPriority.Armut;

if (noSonderkarten) return [];

// SchlankerMartin: Genscherdamen ausblenden
bool isSchlankerMartin = reservation?.Priority == ReservationPriority.SchlankerMartin;
return GetEnabled(rules)
    .Where(s =>
        s.TriggeringCard == playedCard.Type
        && s.AreConditionsMet(state)
        && !(isSchlankerMartin && s.Type is SonderkarteType.Genscherdamen or SonderkarteType.Gegengenscherdamen)
    )
    .ToList();
```

### Keine Frontend-Änderungen nötig
`GameQueryService` ruft bereits `SonderkarteRegistry.GetEligibleForCard` auf — die Filterung wirkt sich automatisch auf `eligibleSonderkartenPerCard` im PlayerGameViewResponse aus.
