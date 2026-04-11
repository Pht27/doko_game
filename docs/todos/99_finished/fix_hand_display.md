# Fix: Hand display clips selected/emphasized cards at the top

## Problem

When a card is selected or hovered on PC, it lifts upward (via `translateY`). The `.hand-container`
uses `height + overflow: hidden` to clip the bottom half of cards (the "rising from table" effect).
That same `overflow: hidden` clips lifted cards at the **top** of the container too.

## Root Cause

`.hand-container` in [HandDisplay.css](src/frontend/src/styles/HandDisplay.css):
- `height: 4rem` (mobile) / `6rem` (tablet+) — sets the visible window
- `overflow: hidden` — clips **both** top and bottom

Max upward travel:
- Selected card (`SELECTED_LIFT_REM = 1.25rem`) — largest lift
- Playable card on hover (`-translate-y-3 = 0.75rem`)

So a selected center card can lift 1.25rem above the container's top edge and gets clipped.

## Plan

Add `padding-top: 1.5rem` + `margin-top: -1.5rem` to `.hand-container`, and increase `height` by
1.5rem to compensate (border-box, so height includes the padding):

```
Mobile   : height 4rem  → 5.5rem, padding-top: 1.5rem, margin-top: -1.5rem
Tablet+  : height 6rem  → 7.5rem, padding-top: 1.5rem, margin-top: -1.5rem
```

**Why this works:**
- `padding-top` shifts the cards 1.5rem down inside the container → same visible card area (4rem / 6rem)
- `margin-top: -1.5rem` pulls the container up by the same amount → net layout height unchanged, bottom clipping unchanged ✓
- The 1.5rem "buffer" zone above the card content sits over the game-table area (transparent background shows through)
- Cards lifting into that zone render over the table background — the desired effect ✓
- `overflow: hidden` still clips the bottom of cards ✓

**Only file changed:** `src/frontend/src/styles/HandDisplay.css`
