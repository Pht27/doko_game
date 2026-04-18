Der Player Switcher soll abgeschafft werden. Es soll im Code auf oberster Ebene einen Flag geben, ob PlayerSwitching erlaubt ist.

Falls ja, soll ein Klicken auf das Label eines Spielers zu dessen View wechseln. Falls nein, ist es nicht möglich. Der Flag soll erstmal auf true sein.

Die GameInfo soll dafür nach oben rechts anstatt oben links.

---

## Implementierungsplan

### Was ändert sich:

1. **Flag in `App.tsx`** — `const PLAYER_SWITCHING_ENABLED = true` als Top-Level-Konstante vor der Komponente
2. **`PlayerLabel.tsx`** — optionaler `onClick?: () => void` prop; wenn gesetzt, als `<button>` gerendert mit Cursor-Pointer und Hover-Effekt
3. **`GameBoard.tsx`**:
   - `PlayerSwitcher` entfernen (Import + JSX)
   - `GameInfo` von `top-2 left-2` → `top-2 right-2`
   - neues Prop `allowPlayerSwitching: boolean`
   - `onClick={() => onPlayerSwitch(player.id)}` an alle drei `PlayerLabel`s wenn Flag true
4. **`useGameActions.ts`** — Auto-Advance entfernen: `setActivePlayer((activePlayer + 1) % 4)` in `handleHealthCheck`, `handleReservation` und `handleArmutResponse` entfernen; `setActivePlayer` nicht mehr als Parameter
5. **`PlayerSwitcher`-Dateien löschen** — `PlayerSwitcher.tsx` und `PlayerSwitcher.css`

### Betroffene Dateien:
- `src/frontend/src/App.tsx`
- `src/frontend/src/components/GameBoard/GameBoard.tsx`
- `src/frontend/src/components/shared/PlayerLabel.tsx`
- `src/frontend/src/styles/PlayerLabel.css` (hover-Stil für klickbare Labels)
- `src/frontend/src/hooks/useGameActions.ts`
- `src/frontend/src/components/shared/PlayerSwitcher.tsx` (löschen)
- `src/frontend/src/styles/PlayerSwitcher.css` (löschen)
