When in an Armut the rich player recieves 5 cards and has to choose which cards to give back, the hand gets too big for the screen. maybe we can adjust the card size in the edge cases 4 and 5 trumps

## Plan

### Problem

In Doppelkopf Armut the rich player temporarily holds 15–17 cards (12 + 3–5 received).  
At the current mobile card step of **1.68 rem** that fan overflows a 375 px viewport.

### Solution

Threshold-based: if `cards.length >= 13` snap to a smaller fixed step; otherwise leave the layout untouched.  
Card height stays the same — only the horizontal overlap increases (cards fan more tightly).

| Tier    | Threshold     | Mobile step | Tablet step |
|---------|---------------|-------------|-------------|
| Normal  | < 13 cards    | 1.68 rem    | 2.46 rem    |
| Compact | ≥ 13 cards    | 1.0 rem     | 1.8 rem     |

Compact mobile: 17 cards → 16 × 1.0 + 5.18 = **21.2 rem (339 px)** — fits in 375 px.  
Compact tablet: 17 cards → 16 × 1.8 + 7.76 = **36.6 rem (585 px)** — fits in 640 px.

### Files affected

- `src/frontend/src/components/HandDisplay/handDisplay.constants.ts`  
  Add `MOBILE_COMPACT_CARD_STEP_REM = 1.0`, `TABLET_COMPACT_CARD_STEP_REM = 1.8`, `COMPACT_HAND_THRESHOLD = 13`.

- `src/frontend/src/components/HandDisplay/HandDisplay.tsx`  
  Update `useCardStep(cardCount: number)`: return compact step when `cardCount >= COMPACT_HAND_THRESHOLD`.
