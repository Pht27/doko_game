TrickArea muss immer zentriert sein, egal wie groß die Player Label sind.

PlayerLabel sollen nicht die Anzahl der Karten anzeigen. Allgemein würde ich das Player Label eher vertikal gestalten. Es soll anzeigen: Name, Ansagen und Known Party (wie jetzt schon). Das Sonderkarten-Anzeige Flash kann so bleiben wie es ist.

OwnPartyLabel soll komplett entfernt werden, denn das kann das Kontrasolo verraten. Die Spieler müssen sich ihre Partei halt merken.

---

## Implementation Plan

### 1. Fix TrickArea centering — `PlayerGrid.tsx`

Current middle row uses `flex items-center justify-between`. If left and right PlayerLabels have different widths, the center drifts off-center.

**Fix:** Switch to `grid grid-cols-[1fr_auto_1fr]` — left and right columns each get equal space (`1fr`), center column is sized to its content (`auto`). This guarantees the TrickArea is always perfectly centered regardless of label sizes.

```diff
- <div className="flex items-center justify-between w-full px-6">
+ <div className="grid grid-cols-[1fr_auto_1fr] items-center w-full px-2">
    <div className="flex justify-start">{left}</div>
    {center}
    <div className="flex justify-end">{right}</div>
  </div>
```

### 2. Redesign PlayerLabel — `PlayerLabel.tsx` + `PlayerLabel.css`

- Remove `<span>{t.kartenAnzahl(player.handCardCount)}</span>` (card count)
- Remove orientation-specific flex-row layouts (`player-label-left` → flex-row, `player-label-right` → flex-row-reverse)
- All orientations become vertical (`flex-col`, which is already the base)
- The orientation prop can be kept for potential future use or removed (keep it to avoid breaking the interface, just make all classes resolve to the same flex-col)

Display order (vertical, top-to-bottom):
1. Player name (bold)
2. Party dot
3. Announcement badge (conditional)
4. Sonderkarte flash (conditional, as-is)

### 3. Remove OwnPartyLabel — `GameBoard.tsx`, `OwnPartyLabel.tsx`, `OwnPartyLabel.css`

- Remove import of `OwnPartyLabel` in `GameBoard.tsx`
- Remove `{view?.ownParty && <OwnPartyLabel party={view.ownParty} />}` render line
- Delete `src/frontend/src/components/shared/OwnPartyLabel.tsx`
- Delete `src/frontend/src/styles/OwnPartyLabel.css`
