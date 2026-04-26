# Lobby Leave & Rejoin

## What to build

1. **Leave button in the game info overlay** (top-right badge → overlay): "Lobby verlassen" triggers a confirmation dialog. Confirming returns the player to the lobby browser with their lobby selected and their seat now empty.
2. **Leave button in the result screen** (post-game): same button + same confirmation next to the "Bereit" button.
3. **Joinable empty seats during a running game**: when a seat becomes free (player left), anyone in the lobby browser can click it to join and pick up the game. Same player can rejoin their own seat.
4. **Last player leaves → lobby closes**: already works via existing `TryRemovePlayer` + `LeaveLobbyHandler`.

---

## Implementation Plan

### Backend (3 files)

**`LobbyState.cs` — allow joining started lobbies**
- Remove the `if (IsStarted) return false;` guard from `TryOccupySeat`
- Seats can be claimed even mid-game as long as they are empty

**`LobbyResponses.cs` — expose active game ID**
- Add `ActiveGameId` (nullable `string`) to `LobbyViewResponse`

**`LobbiesController.cs` — return active game ID**
- In `GET /lobbies/{id}`: return `lobby.ActiveGameId?.ToString()` in the response

---

### Frontend (7 files)

**`src/api/lobby.ts`**
- Add `activeGameId?: string | null` to `LobbyViewResponse` type

**`src/hooks/useLobby.ts`**
- Add `isStarted: boolean` to `LobbyHookState`
- In the initial `getLobby` fetch: if `isStarted && activeGameId`, immediately set `gameId` state (so a re-joining player is auto-navigated to the game once they have a session)

**`src/components/MultiplayerBrowserPage/LobbyDetailView.tsx`**
- Add `session` to the `useEffect([gameId])` dependency array so navigation fires when session is set *after* `gameId` is already known (mid-game rejoin flow)
- When `isStarted`: hide the "Bereit" voting button, show a "Spiel läuft" badge
- Empty seats remain clickable/joinable even when the game is running

**`src/components/shared/GameInfoOverlay.tsx`**
- Add `onLeaveLobby?: () => void` prop
- Add a "Lobby verlassen" button (below the close button) — only rendered when `onLeaveLobby` is provided

**`src/components/ResultScreen/ResultScreen.tsx`**
- Add `onLeaveLobby?: () => void` prop
- When set: render a "Lobby verlassen" button
  - In multiplayer mode: next to (or below) the Bereit button
  - In viewOnly mode: alongside the "Schließen" button

**`src/components/GameBoard/GameBoard.tsx`**
- Add `onLeaveLobby?: () => Promise<void>` prop
- Add `showLeaveConfirm: boolean` state
- When no `lastFinishedResult`: clicking the info badge shows `GameInfoOverlay` (with `onLeaveLobby` wired to `() => setShowLeaveConfirm(true)`)
- When `lastFinishedResult` exists: shows `ResultScreen` in viewOnly mode (with `onLeaveLobby` wired the same way)
- Render a confirmation overlay when `showLeaveConfirm`:
  - "Willst du die Lobby wirklich verlassen?"
  - "Ja" → close confirm, call `onLeaveLobby()`
  - "Nein" → close confirm
- Pass `onLeaveLobby={() => setShowLeaveConfirm(true)}` into `GameOverlays` → `ResultScreen` for the post-game result screen as well

**`src/App.tsx`**
- Add `handleLeaveLobby` function:
  - Calls `leaveLobby(session.token, session.lobbyId)` (best-effort, ignore errors)
  - Navigates to `{ kind: 'multiplayer-browser', selectedLobbyId: lobbySession.lobbyId }`
- Pass `onLeaveLobby={handleLeaveLobby}` to `GameBoard` only when `view.lobbySession` exists (not in hot-seat mode)

---

## Data flow: mid-game rejoin

```
Player opens lobby browser
→ clicks started lobby with empty seat
→ LobbyDetailView mounts
→ useLobby fetches lobby: isStarted=true, activeGameId="abc"
→ sets gameId="abc" in state
→ player clicks empty seat → doJoin(i) → gets token + session
→ setSession(session) triggers useEffect([gameId, session?.token])
→ gameId && session → onGameStarted("abc", session) → navigates to game
```

## Data flow: leave during game

```
Player clicks info badge → GameInfoOverlay (or ResultScreen viewOnly)
→ clicks "Lobby verlassen" → onLeaveLobby() fires
→ GameBoard shows confirmation overlay
→ player clicks "Ja"
→ App.handleLeaveLobby: calls leaveLobby API
  → backend: seat becomes null; if last player → lobby deleted + lobbyClosed broadcast
→ App: navigate to multiplayer-browser with lobbyId selected
→ Lobby browser shows lobby with empty seat
```
