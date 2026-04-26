# Lobby Ready System + Match History via Game Info

## What needs to change

### Task 1: Lobby Start Button → All-Must-Vote System
Replace the single-player "Start Game" button with a "Bereit"/"Zurückziehen" voting system
identical to the ResultScreen: all 4 players must vote before the game starts automatically.

### Task 2: Game Info → Show Results Screen (Match History)
Replace the `GameInfoOverlay` (which shows mode + standings) with the `ResultScreen`.
When clicking the GameInfo button (top-right during a game), show the ResultScreen with
the last completed game's match history. Also add a "Spielverlauf" button in the lobby
for the same purpose.

---

## Affected Files

### Backend
- `Code/backend/Doko.Domain/Lobby/LobbyState.cs` — add `_lobbyStartVoters` set + methods
- `Code/backend/Doko.Api/Controllers/LobbiesController.cs` — add `/ready` and `/ready/withdraw` endpoints
- `Code/backend/Doko.Api/DTOs/Responses/LobbyViewResponse.cs` — add `startVoteCount` field

### Frontend
- `Code/frontend/src/api/lobby.ts` — add `voteReady()`, `withdrawReady()`
- `Code/frontend/src/hooks/useLobby.ts` — track `startVoteCount`, listen to `lobbyReadyVoteChanged`
- `Code/frontend/src/components/MultiplayerBrowserPage/LobbyDetailView.tsx` — replace Start button with Bereit/Zurückziehen
- `Code/frontend/src/App.tsx` — track `lastFinishedResult`, pass to GameBoard + MultiplayerBrowserPage
- `Code/frontend/src/components/GameBoard/GameBoard.tsx` — clicking GameInfo shows ResultScreen instead of GameInfoOverlay; add `lastFinishedResult` prop
- `Code/frontend/src/components/ResultScreen/ResultScreen.tsx` — add `viewOnly?: boolean` prop → shows "Schließen" instead of action button
- `Code/frontend/src/components/MultiplayerBrowserPage/MultiplayerBrowserPage.tsx` — pass `lastFinishedResult` down
- Remove: `GameInfoOverlay` usage in GameBoard (component can stay but won't be used)

---

## Implementation Plan

### Backend

#### 1. `LobbyState.cs`
Add a separate `_lobbyStartVoters` HashSet (distinct from `_newGameVoters` which is for post-game voting):
```csharp
private readonly HashSet<byte> _lobbyStartVoters = [];
public int LobbyStartVoteCount => _lobbyStartVoters.Count;
public bool AddLobbyStartVote(PlayerId p) { _lobbyStartVoters.Add(p.Value); return _lobbyStartVoters.Count >= 4; }
public void RemoveLobbyStartVote(PlayerId p) => _lobbyStartVoters.Remove(p.Value);
public void ResetLobbyStartVotes() => _lobbyStartVoters.Clear();
```

#### 2. `LobbyViewResponse`
Add `int StartVoteCount` to the DTO so fresh joiners see current vote count.

#### 3. `LobbiesController`
Add two new endpoints:
- `POST /lobbies/{lobbyId}/ready` — player votes ready to start
  - Requires auth + player in lobby + lobby not yet started + lobby is full
  - Calls `AddLobbyStartVote()` → broadcasts `lobbyReadyVoteChanged: { count }` to lobby group
  - When all 4 ready: calls `ResetLobbyStartVotes()` + same start logic as existing `/start`
    → broadcasts `gameStarted: { gameId }` to lobby group
- `POST /lobbies/{lobbyId}/ready/withdraw` — withdraw ready vote
  - Calls `RemoveLobbyStartVote()` → broadcasts `lobbyReadyVoteChanged: { count }` to lobby group

Keep existing `/start` endpoint as-is (backward compatible).

### Frontend

#### 4. `api/lobby.ts`
Add:
```typescript
export function voteReady(token: string, lobbyId: string): Promise<{ voteCount: number }>
export function withdrawReady(token: string, lobbyId: string): Promise<{ voteCount: number }>
```

#### 5. `hooks/useLobby.ts`
- Add `startVoteCount: number` to `LobbyHookState`
- Initialize from `LobbyViewResponse.startVoteCount` on fetch
- Listen for `lobbyReadyVoteChanged` SignalR event → update count

#### 6. `LobbyDetailView.tsx`
- Receive `startVoteCount` from `useLobby`
- Replace Start button with Bereit/Zurückziehen button (same style as ResultScreen):
  - Shows `X/4 👤` count
  - Only enabled when lobby is full (4 players) and user has a seat
  - Clicking "Bereit" calls `voteReady()`, "Zurückziehen" calls `withdrawReady()`
  - Local `hasVoted` state (like ResultScreen)

#### 7. `ResultScreen.tsx`
Add `viewOnly?: boolean` prop. When `viewOnly && !multiplayerNewGame`, render a
"Schließen" button instead of "Neues Spiel".

#### 8. `App.tsx`
```typescript
const [lastFinishedResult, setLastFinishedResult] = useState<GameResultDto | null>(null);
useEffect(() => { if (finishedResult) setLastFinishedResult(finishedResult); }, [finishedResult]);
```
Pass `lastFinishedResult` to `GameBoard` and to `MultiplayerBrowserPage`.

#### 9. `GameBoard.tsx`
- Replace `GameInfoOverlay` with ResultScreen when GameInfo clicked
- Add `lastFinishedResult?: GameResultDto` prop
- When `showInfoOverlay && lastFinishedResult`: render `<ResultScreen result={lastFinishedResult} viewOnly onNewGame={() => setShowInfoOverlay(false)} />`
- Remove `GameInfoOverlay` import/usage

#### 10. `MultiplayerBrowserPage.tsx` + `LobbyDetailView.tsx`
- Pass `lastFinishedResult` through to `LobbyDetailView`
- In lobby: if `lastFinishedResult` is non-null, show a "Spielverlauf" button
- Clicking it shows a ResultScreen in view-only mode with close handler

---

## Trade-offs / Notes
- `lastFinishedResult` is in-memory only (React state) — refreshing the page loses it. This is
  acceptable since the ResultScreen is automatically shown when a game ends; the history button is
  just a convenience for reviewing during the next game.
- The old `/start` backend endpoint is kept but no longer called from the frontend.
- `_lobbyStartVoters` is separate from `_newGameVoters` to avoid cross-contamination between
  the pre-game ready vote and the between-game new-game vote.
