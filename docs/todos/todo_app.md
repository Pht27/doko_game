The App.tsx is way too bloated. It uses a lot of hooks which could be separate files, it kind of declares components, which arent real components (like Main Area), that could be put into their own component file. Then we could have a component/folder structure, where the component folder contains folders for each high level component which in turn contains the tsx file and a folder for all the subcomponents that are in that component. ofcourse shared components need their own folder. Like how is there not a component / file for Card, does that make sense?

Also we can define some "wrapper components" that dont necessarily have to be visible on screen but which makes it easier to plan the layout and the single components less convoluted.

Please try and execute this to the best of you ability.

Also some constants in some components could maybe be extracted into separate files (maybe as JSON or what could be an appropriate file type? is there something better for typescript?) SO that theyre clearly separated from the code

---

## Implementation Plan

### What changes and why

App.tsx (361 lines) bundles: 2 animation useEffects + state, 6 handler functions, seat-layout helpers, and all board JSX. Extracting these into hooks and a dedicated GameBoard component leaves App.tsx as a thin orchestrator (~50 lines).

### Target folder structure

```
src/
  hooks/
    useHotSeat.ts            (existing)
    useGameState.ts          (existing)
    useTrickAnimation.ts     (NEW — animation phases extracted from App.tsx)
    useGameActions.ts        (NEW — all game handlers + pending/armut state)
  components/
    GameBoard/
      GameBoard.tsx          (NEW — main board layout, extracted from App.tsx JSX)
      subcomponents/
        ArmutBanner.tsx      (NEW — armut info banner inline div)
        CenterArea.tsx       (NEW — decides which dialog or TrickArea to show)
    Card/
      Card.tsx               (NEW — renders a CardDto as an image)
    TrickArea/
      TrickArea.tsx          (MOVED from components/)
      trickArea.constants.ts (NEW — MAX_TILT_DEG, SEAT_OFFSET, FLY_TRANSLATE, etc.)
    HandDisplay/
      HandDisplay.tsx        (MOVED from components/)
      handDisplay.constants.ts (NEW — FAN_SPREAD_DEG, ARC_DEPTH_REM, etc.)
    dialogs/
      HealthCheckDialog.tsx  (MOVED)
      ReservationDialog.tsx  (MOVED)
      ArmutPartnerDialog.tsx (MOVED)
      ArmutReturnDialog.tsx  (MOVED)
    SonderkarteOverlay/
      SonderkarteOverlay.tsx (MOVED)
    ResultScreen/
      ResultScreen.tsx       (MOVED)
    AnnouncementButton/
      AnnouncementButton.tsx (MOVED)
    shared/
      PlayerLabel.tsx        (MOVED)
      GameInfo.tsx           (MOVED)
      PlayerSwitcher.tsx     (MOVED)
    GameLoader.tsx           (NEW — initial loading/error screen)
  styles/                    (stays flat, import paths updated)
```

### Constants format
Use `.ts` files with `export const` — idiomatic TypeScript, tree-shakeable, supports types. JSON files lack type support and are awkward to import with named exports.

### Files affected
- App.tsx (slimmed to ~50 lines)
- All 12 existing components (moved, imports updated)
- 2 existing hooks (no changes)
- CSS files stay in `styles/`, import paths updated in moved components

### Non-obvious decisions
- `eligibleSonderkarten` prop on HandDisplay is declared but never used inside the component — removed during the move
- CSS stays in `styles/` folder to avoid moving 13 files; only relative import paths updated
