I want the frontend to use more Components where it makes sens. I feel like this is the strength of React and were not making use of that. And for each component i want a separate styling file. The App file for example is too bloated, we should separate it into more sensible components.

also to work on translation / user info: For every message or button label etc I should have a file where I choose what it displays. SO i would have something like keine90announcement: "Keine 90 ansagen".

---

## Implementation Plan

### What needs to change

1. **Create `src/translations.ts`** — a single file with all UI strings as named exports so every label is defined in one place (e.g. `gesund: "Gesund"`, `keine90: "Keine 90"`).

2. **Extract `ArmutReturnDialog` component** — currently an inline `<div>` block inside App.tsx (lines 225–239). Move it to `src/components/ArmutReturnDialog.tsx` + `ArmutReturnDialog.css`.

3. **Add CSS files for all existing components** that don't have one yet:
   - `AnnouncementButton.css`, `ArmutPartnerDialog.css`, `GameInfo.css`, `HealthCheckDialog.css`,
     `PlayerLabel.css`, `PlayerSwitcher.css`, `ReservationDialog.css`, `ResultScreen.css`,
     `SonderkarteOverlay.css`, `TrickArea.css`
   - Each component imports its own CSS file.

4. **Update all components** to import labels from `translations.ts` instead of hardcoding strings.

5. **Update App.tsx** to use `ArmutReturnDialog` and labels from `translations.ts`.

### Files affected

- **New:** `src/frontend/src/translations.ts`
- **New:** `src/frontend/src/components/ArmutReturnDialog.tsx` + `.css`
- **New CSS files:** one per existing component (see above)
- **Modified:** all component `.tsx` files + `App.tsx`

### Trade-offs / decisions

- Translations are plain TS object (no i18n library) — matches the project's no-external-lib style.
- Functions used for interpolated strings (e.g. `armutReturnTitle: (playerId, count) => ...`).
- CSS files are empty or minimal where Tailwind handles everything — they establish the pattern and are the right place to add custom styles later.
- Hochzeit condition keys (`FirstTrick`, `FirstFehlTrick`, `FirstTrumpTrick`) are translated in `translations.ts` via a sub-map.
