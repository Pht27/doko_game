Am unteren Bildschirmrand soll ein sehr kleines Label sein, wo die Partei eines Spielers drauf steht. Es soll leicht transparent und grau sein.

## Plan

- Neue Komponente `OwnPartyLabel` in `src/frontend/src/components/shared/OwnPartyLabel.tsx`
- Absolut positioniert im GameBoard-Root-Div (`position: relative`), unten zentriert
- `z-30` → über den Karten (deren z-index: 0..n), unter den Overlays (z-50)
- Styling: sehr klein (`text-[0.65rem]`), leicht transparent und grau (`text-white/30`)
- `pointer-events-none` → keine Klick-Interferenz mit den Karten
- Das HandDisplay bleibt **völlig unverändert** (kein Layout-Shift)
- Einbindung in `GameBoard.tsx`: `{view?.ownParty && <OwnPartyLabel party={view.ownParty} />}`

### Betroffene Dateien
- **neu:** `src/frontend/src/components/shared/OwnPartyLabel.tsx`
- **geändert:** `src/frontend/src/components/GameBoard/GameBoard.tsx`
