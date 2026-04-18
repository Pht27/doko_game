<This feature was started but im not really happy with the turnout. Maybe you can go over it again and second guess some parts of the Implementation Plan or the requirements I wrote. Now it is the game board variable thats bloated, i think it could be separated into subcomponents. Also, the subcomponent structure is not coherent.>

The App.tsx is way too bloated. It uses a lot of hooks which could be separate files, it kind of declares components, which arent real components (like Main Area), that could be put into their own component file. Then we could have a component/folder structure, where the component folder contains folders for each high level component which in turn contains the tsx file and a folder for all the subcomponents that are in that component. ofcourse shared components need their own folder. Like how is there not a component / file for Card, does that make sense?

Also we can define some "wrapper components" that dont necessarily have to be visible on screen but which makes it easier to plan the layout and the single components less convoluted.

Please try and execute this to the best of you ability.

Also some constants in some components could maybe be extracted into separate files (maybe as JSON or what could be an appropriate file type? is there something better for typescript?) SO that theyre clearly separated from the code

---

## Implementation Plan (revised)

### Phase 1 — initial extraction (done)
App.tsx was broken down into hooks + GameBoard component, bringing it from ~361 to 44 lines.
Hooks extracted: `useTrickAnimation`, `useGameActions`.
Components extracted/moved: GameBoard, Card, TrickArea, HandDisplay, dialogs, SonderkarteOverlay, ResultScreen, AnnouncementButton, shared/*, GameLoader.

### Phase 2 — GameBoard subcomponent cleanup (done)

**Problem:** GameBoard.tsx was still 183 lines, mixing layout, player positioning, overlays, and loading states. `GameLoader.tsx` was the only component not in its own folder.

**Changes:**
- `GameBoard/subcomponents/PlayerGrid.tsx` — pure layout wrapper with named slots (top/left/center/right/bottom). No logic, just the compass flex layout. GameBoard passes opponents as slot content.
- `GameBoard/subcomponents/GameOverlays.tsx` — combines SonderkarteOverlay + ResultScreen (both full-screen overlays). Extracted from GameBoard's bottom.
- `GameLoader/GameLoader.tsx` — moved from flat `GameLoader.tsx` to its own folder, consistent with all other components.
- `GameBoard.tsx` reduced from 183 → ~125 lines.

### Final folder structure
```
components/
  GameBoard/
    GameBoard.tsx
    subcomponents/
      ArmutBanner.tsx
      CenterArea.tsx
      PlayerGrid.tsx       ← NEW (pure layout)
      GameOverlays.tsx     ← NEW (full-screen overlays)
  Card/Card.tsx
  TrickArea/TrickArea.tsx + trickArea.constants.ts
  HandDisplay/HandDisplay.tsx + handDisplay.constants.ts
  AnnouncementButton/AnnouncementButton.tsx
  SonderkarteOverlay/SonderkarteOverlay.tsx
  ResultScreen/ResultScreen.tsx
  GameLoader/GameLoader.tsx  ← MOVED (folder consistency)
  dialogs/                   (flat grouping folder — valid pattern)
  shared/                    (flat grouping folder — valid pattern)
styles/                      (stays flat)
```

### Note on future screens
When the lobby feature is implemented, a `screens/` folder should be introduced:
`screens/HomeScreen/`, `screens/LobbyScreen/`, `screens/GameScreen/`.
App.tsx will act as the screen router. No premature changes needed now.
