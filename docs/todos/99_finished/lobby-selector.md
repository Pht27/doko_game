# Lobby Selector / Multiplayer Browser

## Spec
On the landing page, replace "Lobby erstellen" with a "Mehrspieler" button. Clicking it opens the multiplayer browser:

- **Left panel** (30% width, 70% height): scrollable list of active lobbies + "Lobby erstellen" button below
- **Right panel**: lobby detail view showing a 2×2 seat grid, invite link, join/leave/start controls
- **Back button** returns to the landing page

Clicking a lobby in the list shows its detail — you are not joined yet. Clicking an empty seat joins it. You can only hold a seat in one lobby at a time. If the game starts, you are put into it regardless of which lobby you are currently viewing. If the last player leaves a lobby, it is deleted.

---

## Decisions
- **Host concept removed**: Any registered player can click "Start Game". `isHost` field dropped.
- **Invite URLs keep auto-join**: `?lobby={id}` still joins the next available seat automatically, then drops the user into the multiplayer browser with that lobby selected.
- **Start authorization**: Backend checks caller is any registered player in the lobby (not just seat 0).

---

## Backend

### 1. `LobbyState` → sparse seats array
**File**: `Code/backend/Doko.Domain/Lobby/LobbyState.cs`
- Change `List<LobbyPlayer> _players` → `LobbyPlayer?[4] _seats`
- Remove `HostId` property
- Replace `TryAddPlayer()` with `TryOccupySeat(int seatIndex, out PlayerId playerId)`
- Add `TryRemovePlayer(PlayerId playerId)` → returns bool `isNowEmpty`
- `Players` property: `_seats.Where(s => s != null)`
- `HasPlayer(PlayerId id)` helper for start authorization

### 2. `ILobbyRepository` → add list + delete
**File**: `Code/backend/Doko.Application/Lobbies/ILobbyRepository.cs`
```csharp
Task<IReadOnlyList<LobbyState>> GetAllAsync(CancellationToken ct = default);
Task DeleteAsync(LobbyId id, CancellationToken ct = default);
```

### 3. `InMemoryLobbyRepository` → implement new methods
**File**: `Code/backend/Doko.Infrastructure/Repositories/InMemoryLobbyRepository.cs`
- `GetAllAsync`: return all non-started lobbies from `_store`
- `DeleteAsync`: `_store.TryRemove(id, out _)`

### 4. New handler: `LeaveLobbyHandler`
**File**: `Code/backend/Doko.Application/Lobbies/Handlers/LeaveLobbyHandler.cs`
- Removes player from lobby; if lobby is now empty → `DeleteAsync`
- Broadcasts `playerLeft` (or `lobbyClosed` if deleted) via SignalR

### 5. New handler: `JoinSeatHandler`
**File**: `Code/backend/Doko.Application/Lobbies/Handlers/JoinSeatHandler.cs`
- Validates seat index is 0–3, not occupied, lobby not started
- Calls `TryOccupySeat(seatIndex)`

### 6. `LobbiesController` — new + updated endpoints
**File**: `Code/backend/Doko.Api/Controllers/LobbiesController.cs`

| Endpoint | Change |
|---|---|
| `GET /lobbies` | **New** — returns all active lobbies with seat occupancy |
| `POST /lobbies/{id}/seats/{seat}/join` | **New** — join a specific seat |
| `POST /lobbies/{id}/leave` | **New** — unregister (auth required) |
| `GET /lobbies/{id}` | **Updated** — return `bool[4] seats` instead of `playerCount` |
| `POST /lobbies` | Keep as-is (creates lobby, creator auto-occupies seat 0) |
| `POST /lobbies/{id}/start` | **Updated** — replace host check with `lobby.HasPlayer(callerId)` |

### 7. New DTOs
- `LobbyListItemResponse(string LobbyId, bool[] Seats)`
- Update `LobbyViewResponse` to include `bool[] Seats`

### 8. SignalR events
- `playerLeft` — broadcast to `lobby_{id}` group when someone leaves a seat
- `lobbyClosed` — broadcast to `lobby_{id}` group when lobby is deleted
- Lobby list updates via polling (no global SignalR group needed)

---

## Frontend

### 1. `LandingPage` — rename button
**File**: `Code/frontend/src/components/LandingPage/LandingPage.tsx`
- Replace `onCreateLobby` prop with `onMultiplayer`

### 2. New `MultiplayerBrowserPage`
**File**: `Code/frontend/src/components/MultiplayerBrowserPage/MultiplayerBrowserPage.tsx`

```
┌─────────────────────────────────────────────────────┐
│ ← Zurück                                            │
│                                                     │
│ ┌──────────────────┐ ┌─────────────────────────────┐│
│ │ Lobby A   2/4    │ │  [Sitz 0] [Sitz 1]          ││
│ │ Lobby B   3/4    │ │  [Sitz 2] [Sitz 3]          ││
│ │ Lobby C   1/4    │ │                             ││
│ │                  │ │  🔗 link  [Kopieren]         ││
│ │                  │ │  [Verlassen]  [Starten]      ││
│ └──────────────────┘ └─────────────────────────────┘│
│ [Lobby erstellen]                                   │
└─────────────────────────────────────────────────────┘
```

- Left panel: polls `GET /lobbies` every 3 s
- Right panel: `LobbyDetailView` for selected lobby
- "Lobby erstellen" → calls `createLobby()`, auto-joins seat 0, selects new lobby

### 3. New `LobbyDetailView`
**File**: `Code/frontend/src/components/MultiplayerBrowserPage/LobbyDetailView.tsx`
- 2×2 seat grid: seat 0 top-left, seat 1 top-right, seat 2 bottom-left, seat 3 bottom-right
  - Occupied: green dot + "Spieler N"
  - Empty + no active session: clickable → `joinSeat(lobbyId, seatIndex)`
  - User's own seat: "(Du)" marker
- Invite link + copy button
- "Platz verlassen" — shown if user has a seat in this lobby
- "Spiel starten" — shown if user has a seat in this lobby
- SignalR subscription for `playerJoined`, `playerLeft`, `gameStarted`, `lobbyClosed`

### 4. API layer
**File**: `Code/frontend/src/api/lobby.ts`
- `listLobbies()` → `GET /lobbies`
- `joinSeat(lobbyId, seatIndex)` → `POST /lobbies/{lobbyId}/seats/{seatIndex}/join`
- `leaveLobby(token, lobbyId)` → `POST /lobbies/{lobbyId}/leave`
- Update `LobbyViewResponse` type to include `seats: boolean[]`

### 5. `App.tsx` — routing
**File**: `Code/frontend/src/App.tsx`
- New view kind: `{ kind: 'multiplayer-browser'; selectedLobbyId?: string }`
- `home` → `multiplayer-browser` on "Mehrspieler" click
- `?lobby={id}` → keep `'joining'` view; after auto-join → `'multiplayer-browser'` with `selectedLobbyId` set
- `gameStarted` event transitions to game regardless of which lobby is currently viewed
- Remove old `'lobby'` view kind

### 6. `LobbySession` — seat index, no isHost
**File**: `Code/frontend/src/hooks/useLobby.ts`
- Add `seatIndex: number`; remove `isHost`
- Extend hook to handle `playerLeft` and `lobbyClosed` events

### 7. Translations
**File**: `Code/frontend/src/translations.ts`
- Add: `multiplayer`, `leaveSeat`, `noLobbiesAvailable`, `seatLabel`

---

## "One active lobby" enforcement
No global player identity exists on the backend. Enforcement is frontend-only via sessionStorage: if a `LobbySession` is stored, hide seat join buttons on all other lobbies. A user with two tabs could hold multiple seats — acceptable for this casual use case.

---

## Implementation Plan

### Backend changes

#### 1. `LobbyState.cs`
- Replace `List<LobbyPlayer> _players` with `LobbyPlayer?[] _seats = new LobbyPlayer?[4]`
- Remove `HostId` property (use `_seats[0]` internally where needed)
- Update `IsFull`: `_seats.All(s => s != null)`
- Update `Players`: `_seats.Where(s => s != null).Cast<LobbyPlayer>()`
- Add `Seats` property returning a copy of the array for serialization
- Keep `Create()` — auto-occupies seat 0 with `new PlayerId(0)`
- Replace `TryAddPlayer()` with `TryOccupySeat(int seatIndex, out PlayerId playerId)` — validates index 0–3, seat not taken, lobby not full
- Add `TryRemovePlayer(PlayerId playerId)` — sets `_seats[playerId.Value] = null`; returns `bool isNowEmpty` (all null)
- Add `HasPlayer(PlayerId id)` — `_seats[id.Value] != null`

#### 2. `CreateLobbyHandler.cs`
- Remove `lobby.HostId` reference; return `new PlayerId(0)` directly since seat 0 is always the creator.

#### 3. `ILobbyRepository.cs`
- Add `Task<IReadOnlyList<LobbyState>> GetAllAsync(CancellationToken ct = default)`
- Add `Task DeleteAsync(LobbyId id, CancellationToken ct = default)`

#### 4. `InMemoryLobbyRepository.cs`
- Implement `GetAllAsync`: filter out started lobbies from `_store`
- Implement `DeleteAsync`: `_store.TryRemove(id, out _)`

#### 5. `LobbyError.cs`
- Add `SeatOccupied`, `PlayerNotInLobby`
- Remove `NotHost` (no longer used)

#### 6. New `JoinSeatHandler.cs`
- Command: `JoinSeatCommand(LobbyId, int SeatIndex)`
- Result: `JoinSeatResult(PlayerId, bool IsNowFull)`
- Validates: lobby exists, not started, seat 0–3, seat not occupied → `TryOccupySeat`

#### 7. New `LeaveLobbyHandler.cs`
- Command: `LeaveLobbyCommand(LobbyId, PlayerId)`
- Result: `LeaveLobbyResult(bool LobbyDeleted)`
- Calls `TryRemovePlayer`; if `isNowEmpty` → `DeleteAsync`

#### 8. `LobbyResponses.cs`
- Add `LobbyListItemResponse(string LobbyId, bool[] Seats)`
- Update `LobbyViewResponse` → add `bool[] Seats` field

#### 9. `LobbiesController.cs`
- `GET /lobbies` — returns `LobbyListItemResponse[]` via `GetAllAsync`
- `POST /lobbies/{id}/seats/{seat}/join` — calls `JoinSeatHandler`, returns JWT token (AllowAnonymous)
- `POST /lobbies/{id}/leave` — `[Authorize]`, calls `LeaveLobbyHandler`, broadcasts `playerLeft` or `lobbyClosed`
- `GET /lobbies/{id}` — updated to return `Seats` bool array
- `POST /lobbies/{id}/start` — replace host check with `lobby.HasPlayer(callerId)`
- Keep `POST /lobbies` as-is (create lobby, creator auto-sits in seat 0)
- Remove old `POST /lobbies/{id}/join` (replaced by seat-specific join)

#### 10. `ServiceCollectionExtensions.cs`
- Register `JoinSeatHandler` and `LeaveLobbyHandler`

---

### Frontend changes

#### 1. `translations.ts`
- Add: `multiplayer`, `leaveSeat`, `noLobbiesAvailable`, `seatLabel`, `back`, `occupiedByPlayer`

#### 2. `api/lobby.ts`
- Add `LobbyListItemResponse { lobbyId, seats: boolean[] }`
- Update `LobbyViewResponse` to include `seats: boolean[]`
- Add `listLobbies()` → `GET /lobbies`
- Add `joinSeat(lobbyId, seatIndex)` → `POST /lobbies/{id}/seats/{seat}/join`
- Add `leaveLobby(token, lobbyId)` → `POST /lobbies/{id}/leave`
- Remove `joinLobby()` (old join without seat selection)

#### 3. `useLobby.ts`
- Update `LobbySession`: remove `isHost`, add `seatIndex: number`
- Extend hook to handle `playerLeft { seatIndex }` and `lobbyClosed` events
- Add `lobbyLeft: boolean` state (for when user's lobby is deleted)

#### 4. `LandingPage.tsx`
- Rename prop `onCreateLobby` → `onMultiplayer`
- Rename button label to `t.multiplayer`

#### 5. New `MultiplayerBrowserPage.tsx`
- Layout: full-screen, landscape-optimized, 2-panel side-by-side
  - Left 30%: scrollable lobby list + "Lobby erstellen" button
  - Right 70%: `LobbyDetailView` for selected lobby (or placeholder if none)
- Back button (top-left) → returns to home
- Polls `listLobbies()` every 3 s
- "Lobby erstellen" → `createLobby()` → auto-selects new lobby, creator is in seat 0

#### 6. New `LobbyDetailView.tsx`
- 2×2 seat grid: seat 0 top-left, 1 top-right, 2 bottom-left, 3 bottom-right
- Occupied: green dot + "Spieler N"; own seat: "(Du)" marker
- Empty seat + user not yet in lobby: clickable → `joinSeat()`
- Invite link + copy button
- "Platz verlassen" — if user has a seat in this lobby
- "Spiel starten" — if user has a seat in this lobby, any seated player can start
- SignalR: `playerJoined` / `playerLeft` / `gameStarted` / `lobbyClosed`

#### 7. `App.tsx`
- New view: `{ kind: 'multiplayer-browser'; selectedLobbyId?: string }`
- Remove `{ kind: 'lobby' }` view
- `home` → `multiplayer-browser` on "Mehrspieler" click
- `?lobby={id}` auto-join: fetch lobby state, pick first empty seat, call `joinSeat`, then go to `multiplayer-browser` with that lobby selected
- `gameStarted` event in `MultiplayerBrowserPage` transitions to `{ kind: 'game' }`
- Remove `useLobby` from App (move to `LobbyDetailView`)

---

### Key decisions
- **Mobile-first landscape**: panels are side-by-side (this is a landscape-locked app); left `w-[30%]`, right `w-[70%]`
- **Session enforcement**: frontend-only via sessionStorage (one seat at a time per tab)
- **Invite URL auto-join**: fetch lobby, find first empty seat, call `joinSeat` with that index
- **Old `LobbyPage.tsx`**: replaced by `LobbyDetailView`, will be deleted
- **Old `JoinLobbyHandler`**: replaced by `JoinSeatHandler`, will be removed

---

## Verification
1. Tab 1: Landing → Mehrspieler → Lobby erstellen → auto-sits in seat 0 → sees 2×2 grid
2. Tab 2: Landing → Mehrspieler → sees lobby in list → clicks it → clicks empty seat → joins
3. Tab 1 sees seat update in real-time (SignalR)
4. Tab 2: "Platz verlassen" → seat clears in both tabs
5. Both players seated → either clicks "Spiel starten" → both redirect to game
6. Tab 1 leaves as last player → lobby disappears from Tab 2's list
7. Visit `?lobby={id}` URL → auto-joins → lands in browser with lobby selected
