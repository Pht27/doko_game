# Fix: Hand display re-layout on card play

When a card gets played the hand shrinks and the arch of the hand display has to be recalculated. this could use a short animation or transition so it does not look janky.

## Root cause

The `.card-wrapper` already has `transition: transform 250ms ease-in-out`, so the **arc drop** (vertical) and **rotation** changes animate smoothly when card count changes.

The **horizontal** repositioning does NOT animate: it is driven by CSS flexbox `justify-center` reflow, which is instant. When a card is removed, the whole group shifts left/right by `~(cardWidth - overlap) / 2` with no transition, causing a visible snap.

## Plan

Convert the hand from a flex layout to an **absolute-positioned** layout where **all** positioning — X offset, arc drop, and rotation — lives in the `transform` property of each `.card-wrapper`. Since `transition: transform` already exists, everything (including horizontal repositioning) will animate for free.

### Mechanics

- `.hand` becomes `position: relative` with explicit heights matching the card heights (8rem mobile / 12rem tablet).
- Each `.card-wrapper` is `position: absolute; left: 50%; bottom: 0`.  
  `left: 50%` anchors the left edge of each card at the hand's midpoint.
- The JS-computed transform includes a new horizontal term:
  `translateX(calc(xOffset_rem - 50%))` — the `-50%` re-centers the card on its own width, `xOffset` shifts it to its fan position from the center of the hand.
- `xOffset = (index − (total−1)/2) × cardStep`  
  where `cardStep = cardWidth − overlap` (the horizontal spacing between adjacent card centers).

### Card dimensions (from SVG viewBox `110 × 170` and existing CSS)

| Breakpoint | Height | Width (110/170 ≈ 0.647) | Overlap | Step |
|------------|--------|--------------------------|---------|------|
| Mobile     | 8 rem  | ≈ 5.18 rem               | 3.5 rem | ≈ 1.68 rem |
| Tablet 640px+| 12 rem | ≈ 7.76 rem              | 5.3 rem | ≈ 2.46 rem |

### Files affected

| File | Change |
|------|--------|
| `handDisplay.constants.ts` | Add `CARD_ASPECT_RATIO`, `MOBILE_CARD_STEP_REM`, `TABLET_CARD_STEP_REM`, `TABLET_BREAKPOINT_PX` |
| `HandDisplay.tsx` | Add `useCardStep()` hook for responsive breakpoint; update `getCardTransform()` to include X offset |
| `HandDisplay.css` | `.hand` → `position: relative` + explicit heights; `.card-wrapper` → `position: absolute; left: 50%; bottom: 0`; remove `margin-left` rule |

### Non-obvious decisions

- **`left: 50%` fixed in CSS** — the value never changes so it doesn't need to be animated. Only `transform` changes, and that already has a 250ms transition.
- **`-50%` inside `calc()`** — `translateX(-50%)` refers to the element's own width, centering the card on its anchor point. Mixing `rem` and `%` in `calc()` inside a CSS transform is valid and well-supported.
- **`transform-origin: 50% 100%`** is unaffected — it still correctly pins the rotation to the card's own bottom-center regardless of how the card is positioned.
- **z-index** ordering (`zIndex: i`) is unchanged; it works the same for absolute elements.
- **`.hand-container` overflow clipping** is unchanged — it still clips the bottom of cards to create the "cards rising from the table" effect.
