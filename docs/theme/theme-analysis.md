# Theme Analysis — Doppelkopf Frontend

## Summary

The app uses a single dark-navy theme. Before this refactor, every CSS module
re-declared the same raw color values in a local `:root` block. There were **9
separate `:root` blocks** across 9 CSS files all setting identical values for
surface, text, border, and primary colors. This analysis documents the full
color inventory, the resulting semantic mapping, and what was already fixed vs.
what remains as future refactor opportunities.

---

## Detected Color Palette

### Backgrounds / Surfaces

| Raw value | Token | Role |
|---|---|---|
| `#1a1a2e` | `--app-bg` | Page background |
| `#22223a` | `--app-surface` | Cards, panels, headers |
| `#2a2a48` | `--app-surface-2` | Elevated inputs, inner surfaces |
| `#12122a` | `--app-deep` | Deep overlay backgrounds |
| `#1a1a38` | — | LeaderboardGraphOverlay header (near-`--app-bg`) |
| `#16163a` | — | LeaderboardGraphOverlay controls (near-`--app-bg`) |
| `#1e1e38` | — | PlayerPickerModal bg (between `--app-bg` and `--app-surface`) |
| `rgb(45,38,71)` | — | SelfPlayerLabel bg (purple-tinted) |
| `#0f172a` | — | LastTrickOverlay (Tailwind slate-900) |
| `#252540` | — | RulesPage open-section bg |

### Text

| Raw value | Token | Role |
|---|---|---|
| `#eeeeee` | `--app-text` | Primary text |
| `#aaaacc` | `--app-text-sub` | Secondary / descriptions |
| `#666688` | `--app-text-muted` | Muted labels, placeholders |
| `#e2e2e2` | — | RulesPage black-suit color (not a semantic role) |
| `#f0f0ff` | — | GameAnnouncePopup text (one-off near-white) |

### Borders

| Raw value | Token | Role |
|---|---|---|
| `rgba(255,255,255,0.08)` | `--app-border` | Default subtle border |
| `rgba(255,255,255,0.12)` | — | Slightly stronger (often inline) |
| `rgba(255,255,255,0.14)` | `--app-border-md` | Medium border |
| `rgba(255,255,255,0.20)` | `--app-border-lg` | Clearly visible border |

### Primary Action

| Raw value | Token | Role |
|---|---|---|
| `#4f46e5` | `--app-primary` | Primary buttons / active tab |
| `#6366f1` | `--app-primary-h` | Primary hover |
| `#4338ca` | — | Primary active (darker) — only in PlayersPage |

### Team Colors

| Raw value | Token | Role |
|---|---|---|
| `#818cf8` | `--app-re` | Re team accent (indigo-400 range) |
| `rgba(99,102,241,0.15)` | `--app-re-soft` | Re soft background |
| `rgba(165,180,252,1)` | `--app-re-label` | Re label text (re-300) |
| `oklch(65% 0.23 303)` | `--app-kontra` | Kontra team accent (purple-400) |
| `oklch(65% 0.23 303 / 0.15)` | `--app-kontra-soft` | Kontra soft background |
| `oklch(78% 0.18 303)` | `--app-kontra-label` | Kontra label text (kontra-300) |

Note: `RulesPage.css` uses `--rp-re: var(--color-re-500)` (blue-500 = `#3b82f6`) for
decorative info boxes, distinct from the game's `--app-re` which is more indigo.
Both are conceptually "Re team blue" but at different shades.

### Status / Semantic

| Raw value | Token | Role |
|---|---|---|
| `#4ade80` | `--app-win` | Win / success (green-400) |
| `#f87171` | `--app-loss` | Loss / error (red-400) |
| `#facc15` | `--app-warning` | Active turn / warning (yellow-400) |
| `#fb923c` | `--app-orange` | Sonderkarte glow, karo suit (orange-400) |
| `#d97706` | `--app-solo` | Solo game mode (amber-600) |

### Chart Palette (LeaderboardPage)

Fixed 8-color palette for player lines — not semantic, purely decorative:

```js
['#818cf8', '#f472b6', '#34d399', '#fbbf24', '#60a5fa', '#fb923c', '#a78bfa', '#4ade80']
```

---

## Grouped Palette (Normalized)

After merging near-equivalents:

**Neutral**
- Deep: `#12122a`
- Bg: `#1a1a2e`
- Surface: `#22223a`
- Surface-2: `#2a2a48`

**Text**
- Primary: `#eeeeee`
- Sub: `#aaaacc`
- Muted: `#666688`

**Primary**
- Base: `#4f46e5`
- Hover: `#6366f1`

**Team Re** (blue/indigo spectrum)
- `#818cf8` (accent) → `var(--color-re-400)`
- `#3b82f6` (rules info) → `var(--color-re-500)`

**Team Kontra** (purple)
- `oklch(65% 0.23 303)` → `var(--color-kontra-400)`
- `oklch(78% 0.18 303)` → `var(--color-kontra-300)`

**Status**
- Win: `#4ade80`
- Loss: `#f87171`
- Solo: `#d97706`
- Warning: `#facc15`
- Orange: `#fb923c`

---

## Semantic Mapping

| Usage | Semantic Token |
|---|---|
| Page background | `--app-bg` |
| Card / panel background | `--app-surface` |
| Input / inner surface | `--app-surface-2` |
| Overlay background | `--app-deep` |
| Body text | `--app-text` |
| Muted / secondary text | `--app-text-sub` |
| Label / metadata text | `--app-text-muted` |
| Default border | `--app-border` |
| Medium border | `--app-border-md` |
| Prominent border | `--app-border-lg` |
| Primary button | `--app-primary` |
| Primary hover | `--app-primary-h` |
| Re team accent | `--app-re` |
| Kontra team accent | `--app-kontra` |
| Solo game mode | `--app-solo` |
| Win / success | `--app-win` |
| Loss / error | `--app-loss` |
| Active / warning | `--app-warning` |
| Sonderkarte glow | `--app-orange` |

---

## What Was Fixed

All 9 CSS files that had local `:root` blocks with duplicated raw values now
reference the global `--app-*` tokens:

- `RoundForm.css` (`--arf-*`)
- `TeamEditorModal.css` (`--tem-*`)
- `GameModePickerModal.css` (`--gmp-*`)
- `HistoryPage.css` (`--ahr-*`)
- `PlayersPage.css` (`--ap-*`)
- `PlayerPage.css` (`--apd-*`)
- `RulesPage.css` (`--rp-*`)
- `LeaderboardPage.css` (`--alb-*`)

Hardcoded `#1a1a2e` in CSS rules replaced with `var(--app-bg)`:
- `RoundForm.css`, `HistoryPage.css`, `PlayersPage.css`, `PlayerPage.css`,
  `RulesPage.css`, `LandingPage.css`

Hardcoded values in TSX replaced with CSS var references:
- `GameBoard.tsx`: `bg-[#1a1a2e]` → `bg-[var(--app-bg)]`
- `PageHeader.tsx`: `bg-[#22223a]` → `bg-[var(--app-surface)]`

---

## Remaining Refactor Opportunities

The following still contain hardcoded colors. Grouped by effort level:

### Low effort — inline styles in TSX

| File | Color | Replace with |
|---|---|---|
| `NotFoundPage.tsx` | `background: '#1a1a2e'` | `var(--app-bg)` |
| `NotFoundPage.tsx` | `color: '#eeeeee'` | `var(--app-text)` |
| `NotFoundPage.tsx` | `rgba(255,255,255,0.45)` | `var(--app-border-lg)` + opacity |
| `BottomSheet.tsx` | `background: '#22223a'` | `var(--app-surface)` |
| `BottomSheet.tsx` | `borderTop: '1px solid rgba(255,255,255,0.08)'` | `var(--app-border)` |
| `BottomSheet.tsx` | `color: '#eeeeee'` | `var(--app-text)` |
| `AnalogEditRoundPage.tsx` | `color: '#aaaacc'` | `var(--app-text-sub)` |
| `AnalogEditRoundPage.tsx` | `color: '#f87171'` | `var(--app-loss)` |
| `AnalogNewRoundPage.tsx` | same as above | same |
| `ExpandableRow.tsx` | `rgba(255,255,255,0.08)` | `var(--app-border)` |
| `Tile.tsx` | `rgba(255,255,255,0.05)` | surface bg |
| `Tile.tsx` | `rgba(255,255,255,0.07)` | border |

### Medium effort — component-level CSS files with raw colors

| File | Colors to replace |
|---|---|
| `GameAnnouncePopup.css` | `rgba(30,30,60,0.92)`, `#f0f0ff`, `rgba(120,140,255,0.7)` |
| `LeaderboardGraphOverlay.css` | `#12122a`, `#1a1a38`, `#16163a`, `#818cf8` |
| `PlayerPickerModal.css` | `#1e1e38`, `#eee`, `#818cf8` |
| `LandingPage.tsx` | `RED_SUIT`, `RE_BLUE`, `KONTRA_PURPLE`, `ORANGE` constants |
| `LeaderboardGraphOverlay.tsx` | multiple `rgba(255,255,255,*)` for chart axis |

### Low priority — specialized use cases

| File | Note |
|---|---|
| `TitleCard.css` | oklch colors for armut/solo/hochzeit badges — intentional, game-mode specific |
| `GameModeBadge.css` | oklch colors for each game mode — intentional |
| `HealthCheckDialog.css` | oklch colors for armut/vorbehalt — intentional |
| `SonderkarteFlash.css` | `rgba(251,146,60,*)` — could use `--app-orange` |
| Chart palette in `LeaderboardPage.tsx` | Fixed palette, acceptable as-is |

---

## Duplicate / Redundant Colors

- `#1a1a2e`, `#1a1a38`, `#16163a`, `#1e1e38` — all variants of the page
  background. Only `#1a1a2e` is canonical; the others appear in the
  LeaderboardGraphOverlay and should eventually map to `--app-bg` or `--app-deep`.

- `rgba(255,255,255,0.08)`, `rgba(255,255,255,0.07)`, `rgba(255,255,255,0.06)` —
  all serve as "subtle border" roles. Unify to `--app-border` = `0.08`.

- `#818cf8` (app-re) and `#3b82f6` (re-500) both used as "Re team" color in
  different contexts, creating visual inconsistency. RulesPage should use
  `var(--app-re)` and drop the custom shade.

---

## Inconsistent Usages

- `SelfPlayerLabel.css` uses `rgb(45, 38, 71)` as a custom purple-tinted
  surface for the bottom player bar — this is intentional but undocumented.
  Could become `--app-self-surface` if theming is desired.

- `RulesPage.css` uses `--rp-re: var(--color-re-500)` while all other contexts
  use `--app-re: #818cf8` (indigo-400). The rules page should align with the
  rest of the app.

---

## Contrast Notes

All text/background combinations use the same dark-navy base (`#1a1a2e`) with
near-white text (`#eeeeee`). Estimated contrast ratios:

- `#eeeeee` on `#1a1a2e` ≈ **12:1** ✓ (WCAG AAA)
- `#aaaacc` on `#1a1a2e` ≈ **5.5:1** ✓ (WCAG AA)
- `#666688` on `#1a1a2e` ≈ **2.4:1** ✗ (below AA — muted text, used only for
  non-essential labels)
- `#818cf8` on `#1a1a2e` ≈ **4.5:1** ✓ (WCAG AA)

---

## How to Create a New Theme

1. Copy `Code/frontend/src/styles/index.css`.
2. Override the `--app-*` tokens in `:root` for the new theme.
3. Optionally override `--color-re-*` and `--color-kontra-*` in `@theme` for
   new team colors.
4. No component CSS files need changes — all derive from global tokens.

The remaining hardcoded values listed above would need to be migrated first for
a 100% theme-switchable UI.
