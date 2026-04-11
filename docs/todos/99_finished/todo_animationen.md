Kleine Animation beim Karte ausspielen, Karte soll von der Hand kurz in die Trick Area fliegen. Auch das neu anordnen der Hand (neu Berechnugn des Fächers) soll smooth sein, das ist momentan etwas hakelig. Außerdem soll so ungefähr die Hälfte der Karten (bei denen ganz außen maximal so 60%) abgeschnitten sein vom unteren Bildschirmrand. Ist das irgendwie möglich, das flexibel für mobile Geräte zu designen?

## Plan

### Drei Probleme

1. **Karte ausspielen – Flug-Animation**  
   Beim Ausspielen soll die Karte aus dem Fächer nach oben (Richtung Trick Area) wegfliegen.

2. **Fächer neu anordnen – smooth**  
   Aktuell `duration-150 transition-transform` auf `.card-wrapper`. 150 ms fühlt sich ruckartig an.

3. **Karten halb abgeschnitten unten**  
   Ziel: Mittelkarten ~50 % sichtbar, Außenkarten max ~60 % abgeschnitten.

---

### Lösung

#### 1. Flug-Animation
- Neuer State `playingCardId: number | null` in `useGameActions`.
- Beim Ausspielen: `playingCardId` sofort setzen (vor API-Call).
- In `submitPlayCard` nach dem API-Response mindestens `PLAY_ANIMATION_MS = 350 ms` warten, bevor `refetch()` aufgerufen wird → Karte bleibt im DOM bis Animation fertig.
- `HandDisplay` erhält neues Prop `playingCardId?` und hängt CSS-Klasse `card-playing` an das SVG der betreffenden Karte.
- `@keyframes card-play-out`: Karte hebt leicht ab → scale 0.75 + translateY(-5rem) + fade out über 350 ms.
- Das SVG-Transform composited mit dem Fächer-Transform des Wrappers → Karte fliegt aus Fächerposition heraus nach oben.

#### 2. Smooth Fächer
- `.card-wrapper` Transition: `transition: transform 300ms ease-in-out` (statt `duration-150`).

#### 3. Karten abschneiden (responsiv)
- `ARC_DEPTH_REM`: 2 → 1 (Außenkarten fallen nur 1 rem statt 2 rem, damit bei 60 %-Abschnitt noch genug sichtbar bleibt).
- `.hand` bekommt `transform: translateY(var(--hand-clip))`:
  - Mobile: `--hand-clip: 4rem` (Karte 8 rem → 50 % abgeschnitten für Mittelkarten ✓)
  - Tablet (≥ 640 px): `--hand-clip: 6rem` (Karte 12 rem → 50 % ✓)
- `GameBoard` hat bereits `overflow: hidden` → der untere Teil wird automatisch abgeschnitten.
- Für Außenkarten: Gesamtversatz = `hand-clip + arc-drop` (4 + 1 = 5 rem) → 3 rem sichtbar von 8 rem = 37 % sichtbar ≈ 62 % abgeschnitten ✓

### Betroffene Dateien
- `src/frontend/src/components/HandDisplay/handDisplay.constants.ts`
- `src/frontend/src/styles/HandDisplay.css`
- `src/frontend/src/components/HandDisplay/HandDisplay.tsx`
- `src/frontend/src/hooks/useGameActions.ts`
- `src/frontend/src/components/GameBoard/GameBoard.tsx`