# Frontend Planning: Refine Sketch & Implementation Plan

## Context

The backend (Doko.Api) is complete — REST endpoints, SignalR hub, DTOs, and tests are all done. The next step is to build a **dev hot-seat frontend**: a single-browser React app that lets one person play all 4 seats sequentially, primarily to exercise and test the backend without needing 4 devices. Real multiplayer frontend is a separate future effort.

The existing `docs/features/frontend/frontend-sketch.md` is informal and incomplete. This plan restructures it into a proper spec and defines the implementation approach.

---

## Decisions Made

| Decision | Choice |
|---|---|
| Scope | Dev hot-seat only (not final multiplayer product) |
| Framework | React + TypeScript |
| Build tool | Vite |
| Styling | Tailwind CSS |
| Auth strategy | Issue all 4 JWT tokens at app startup |
| Announcements | Show button only when `LegalAnnouncements` is non-empty |
| Folder layout | Add `frontend/` at repo root alongside `src/` and `tests/` |

---

## Linux Packages Required

On Manjaro (Arch-based), install Node.js and npm:
```bash
sudo pacman -S nodejs npm
```
Verify:
```bash
node --version   # should be 18+
npm --version
```

---

## Task 1: Rewrite `frontend-sketch.md`

**File:** `docs/features/frontend/frontend-sketch.md`

Replace the informal sketch with a structured spec containing the following sections:

### 1. Overview
- Purpose: hot-seat dev frontend (not final product)
- Target: mobile horizontal landscape (also functional on desktop browser)
- Entry point: single Vite app, talking to `Doko.Api` over HTTP + SignalR

### 2. Tech Stack
```
React 19 + TypeScript
Vite (build & dev server)
Tailwind CSS (utility styling)
@microsoft/signalr (SignalR client)
```
No additional state management library — React Context + useState is sufficient for hot-seat scope.

### 3. Folder Structure (within repo)
```
Doko.sln
src/               (.NET backend projects — unchanged)
tests/             (.NET test projects — unchanged)
frontend/          (new — Vite app root)
  src/
    api/           typed wrappers for HTTP endpoints + SignalR setup
    components/    UI components
    hooks/         useGameState, useSignalR, useHotSeat
    types/         TypeScript types mirroring backend DTOs
    assets/
      cards/       copied card SVGs from /resources/cards/
  public/
  index.html
  vite.config.ts
  tailwind.config.ts
  tsconfig.json
  package.json
docs/
resources/
```

### 4. Hot-Seat Dev Mode

On app start:
1. Call `POST /auth/token` for players 0, 1, 2, 3 — store all 4 tokens in state
2. Call `POST /games` with all 4 player IDs to create a game
3. Auto-deal: immediately call `POST /games/{id}/deal`
4. Render the active player's view via `GET /games/{id}` with that player's token

A persistent **player switcher** (segmented control, top-right corner) lets the user switch active player perspective. On switch: re-fetch `GET /games/{id}` with the new player's token.

All subsequent API calls use the **active player's token**.

### 5. SignalR Integration
- On any event (`CardPlayed`, `TrickCompleted`, `AnnouncementMade`, `ReservationMade`, `GameFinished`, `SonderkarteTriggered`): re-fetch `GET /games/{id}` for the active player to refresh state
- Events drive UI updates (e.g., trick animation trigger), not state hydration directly
- One shared SignalR connection (not per-player) is sufficient for hot-seat dev

### 6. Phases & UI Flows

**Phase: Dealing**
- Auto-deal on game creation (no manual button needed)
- Transition immediately to Reservations

**Phase: Reservations**
- Sequential reservation dialogs, one per player in seat order
- Each dialog shows `EligibleReservations` from that player's view
- Buttons: one per eligible reservation type + "Pass"
- After each declaration: player switcher auto-advances to next player
- After all 4 declare: game transitions to Playing

**Phase: Playing (main game view)**

Layout (landscape, full screen):
```
┌────────────────────────────────────────────────────────────────┐
│ [Game Info: phase, trick #]      [Player Top: name, count] [P0|P1|P2|P3] │
│                         [Top card if played]                    │
│ [Player Left]      [Center Trick Area: 4 compass cards]  [Player Right] │
│                         [Bottom card if played]                 │
│                   [Announce button if eligible]                 │
│         [Hand: stacked card SVGs, bottom row, tap to play]      │
└────────────────────────────────────────────────────────────────┘
```

- **Whose turn indicator**: The label (top/left/right) or hand area of the active-turn player is visually highlighted (e.g., colored border or glow)
- If `IsMyTurn` is true for the current player, hand cards are tappable; otherwise dimmed

**Phase: Finished**
- Show `GameResultDto`: winner party, points, game value, extra awards list
- "New Game" button

### 7. Components

| Component | Responsibility |
|---|---|
| `<PlayerSwitcher>` | Segmented control in top-right corner; shows P0–P3; highlights active seat |
| `<HandDisplay>` | Bottom row of stacked card SVGs; illegal cards dimmed; tap to play |
| `<TrickArea>` | Center area: up to 4 cards in compass positions; wiggle animation on trick complete |
| `<PlayerLabel>` | Name + hand count + known party for one opponent (left/top/right); highlighted when it's that player's turn |
| `<GameInfo>` | Phase label + trick number (top-left) |
| `<ReservationDialog>` | Modal: eligible reservations as buttons + Pass |
| `<AnnouncementButton>` | Floating button; visible only when `LegalAnnouncements.length > 0` |
| `<SonderkarteOverlay>` | Shown after tapping a card with eligible Sonderkarten; list of options **including "None"**; player confirms before card is played |
| `<ResultScreen>` | Game-over overlay with score breakdown |

### 8. Card Assets & Mapping

SVGs are in `/resources/cards/` (copied to `frontend/src/assets/cards/` as static assets).

File naming convention: `<suit><rank>.svg`
- Suits: `kr` (Kreuz), `p` (Pik), `h` (Herz), `k` (Karo)
- Ranks: `9`, `10`, `B` (Bube/Jack), `D` (Dame/Queen), `K` (König/King), `A` (Ass/Ace)

Mapping from DTO to filename:
```ts
const SUIT_MAP: Record<string, string> = {
  Kreuz: 'kr', Pik: 'p', Herz: 'h', Karo: 'k'
};
const RANK_MAP: Record<string, string> = {
  Nine: '9', Ten: '10', Jack: 'B', Queen: 'D', King: 'K', Ace: 'A'
};
function cardSvg(suit: string, rank: string): string {
  return `${SUIT_MAP[suit]}${RANK_MAP[rank]}.svg`;
}
```

Examples: `{ Suit: "Herz", Rank: "Ten" }` → `h10.svg`, `{ Suit: "Kreuz", Rank: "King" }` → `krK.svg`

### 9. API Interaction Pattern
- All requests include `Authorization: Bearer <token>` for the active player
- On error from `POST /games/{id}/cards`: show inline error popup over the hand
- Use `HandSorted` (not `Hand`) for default display order — no sort toggle needed
- No retry logic needed for hot-seat dev scope

### 10. Open Questions (resolved)
- ~~Hot-seat via API or separate test mode?~~ → Use existing API, issue 4 tokens at startup
- ~~Announcement yes/no each turn or button-only?~~ → Button shown only when eligible
- ~~TypeScript?~~ → Yes
- ~~Styling?~~ → Tailwind
- ~~Folder layout?~~ → `frontend/` at repo root alongside `src/` and `tests/`

### 11. Out of Scope (this iteration)
- Real multiplayer (separate session, separate browser tabs)
- Offline support / reconnection resilience
- User accounts or persistent game history
- Trick animation timing details (to be refined during implementation)

---

## Task 2: Implementation Steps (after spec is approved)

1. Install Node.js: `sudo pacman -S nodejs npm`
2. Scaffold `frontend/` at repo root: `npm create vite@latest frontend -- --template react-ts`
3. Install Tailwind CSS, configure `tailwind.config.ts`
4. Install `@microsoft/signalr`
5. Copy card SVGs: `cp resources/cards/* frontend/src/assets/cards/`
6. Write `src/types/api.ts` — TypeScript types for all DTOs (derived from `Doko.Api/DTOs/`)
7. Write `src/api/` — typed HTTP functions + `cardSvg()` mapping utility
8. Write `src/api/signalr.ts` — SignalR client setup
9. Implement `useHotSeat` hook — startup flow (4 tokens, create game, auto-deal)
10. Build components in order: PlayerSwitcher → HandDisplay → TrickArea → PlayerLabels (with turn highlight) → GameInfo
11. Wire playing phase together with `useGameState`
12. Implement ReservationDialog phase (auto-advance player switcher)
13. Implement AnnouncementButton
14. Implement SonderkarteOverlay (with "None" option)
15. Implement ResultScreen
16. Manual end-to-end test: full game from startup → reservations → play → score

---

## Verification

- Run `Doko.Api` locally, run `npm run dev` in `frontend/`, open in browser (landscape orientation or devtools mobile view)
- Complete a full game through the hot-seat UI switching between all 4 players
- Verify reservation phase completes correctly for all 4 players
- Verify trick completion animations trigger on `TrickCompleted` event
- Verify final score screen shows correct result from `GameResultDto`
- Verify illegal card taps show error popup (not a crash)
- Verify Sonderkarte overlay shows "None" as an option
- Verify active-turn player label is visually highlighted
