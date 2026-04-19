# Game Lobbies

Right now, you load the website and youre ingame. As this is later on intended to be a multiplayer game, ie four people from different devices are supposed to be playing together, I kind of need a lobby system.

I only know the idea from playing games myself and do not know how complicated it would be to implement this. What I want is that before the game is started, there should be a lobby.

So what Im picturing is: You load the website and have 2 options. One is to start a game where noone can join and you can play every player (via playerSwitcher). Kind of a test game (maybe we can hind that behind a feature flag to be only visible in the test environment) - this should probably only be enabled in the test environment

The other option is to create a lobby, where you will be waiting for 3 other players. When 3 other players join you can press start game and a game commences.

We should also think about what that means for the frontend, what components are there, how do you get there, what gets shown where.

---

## Implementation Plan

### Overview

Replace the auto-start behavior with a landing page. Users see two options:
1. **Test Game** (dev/staging only, behind `import.meta.env.DEV` Vite flag) — existing hot-seat mode with player switcher
2. **Create Lobby** — starts a waiting room; share a URL so 3 others can join; host presses "Start Game" when full

Frontend state machine: `'home' → 'lobby' → 'game'` (hot-seat is a side path from home).

---

### Backend

#### New Domain: `Doko.Domain/Lobby/`
- **`LobbyId.cs`** — `readonly record struct LobbyId(Guid Value)`, same pattern as `GameId`
- **`LobbyState.cs`** — Aggregate holding:
  - `Id`, `CreatedAt`
  - `List<LobbyPlayer>` (max 4) where `LobbyPlayer` is `record(PlayerId Id, DateTimeOffset JoinedAt)`
  - `HostId` → `Players[0].Id` (first joiner is always host, player 0)
  - `TryAddPlayer(out PlayerId newId)` — appends next seat (0→1→2→3); returns false if full
  - `IsFull` → `Players.Count == 4`

No domain events needed for the lobby (it's ephemeral pre-game state, not a long-lived aggregate).

#### New Application: `Doko.Application/Lobbies/`
- **`ILobbyRepository.cs`** — `GetAsync(LobbyId, ct)` / `SaveAsync(LobbyState, ct)`
- **`Handlers/CreateLobbyHandler.cs`** — creates a new `LobbyState` with one player (seat 0), saves it, returns `(LobbyId, PlayerId=0)`
- **`Handlers/JoinLobbyHandler.cs`** — loads lobby, calls `TryAddPlayer`, saves, returns `(PlayerId, IsFull)` or failure if already full
- **`Queries/LobbyView.cs`** — simple read model: `(LobbyId, int PlayerCount, bool IsStarted, string? GameId)`

#### New Infrastructure: `Doko.Infrastructure/Repositories/`
- **`InMemoryLobbyRepository.cs`** — `ConcurrentDictionary<LobbyId, LobbyState>`, same pattern as `InMemoryGameRepository`

#### New API: `Doko.Api/Controllers/LobbiesController.cs`
Endpoints:

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| `POST` | `/lobbies` | None | Create lobby → returns `{ lobbyId, playerId, isHost, token }` |
| `POST` | `/lobbies/{id}/join` | None | Join lobby → returns `{ lobbyId, playerId, isHost, token }` |
| `GET` | `/lobbies/{id}` | None | Get lobby state → returns `{ lobbyId, playerCount, isStarted }` |
| `POST` | `/lobbies/{id}/start` | JWT | Start game (host only) → calls StartGame + DealCards, broadcasts SignalR event, returns `{ gameId }` |

`POST /lobbies/{id}/start` orchestrates two existing handlers (`IStartGameHandler` + `IDealCardsHandler`), then broadcasts `gameStarted` via SignalR to the lobby group.

#### Token Generation: Extract `ITokenService`
Move JWT generation out of `AuthController` into `Doko.Api/Services/JwtTokenService.cs` (implements `ITokenService`). Both `AuthController` and `LobbiesController` inject `ITokenService`. Registered in `Doko.Api/Extensions/ServiceCollectionExtensions.cs`.

#### SignalR: Extend `GameHub`
Add two methods to the existing `GameHub`:
```csharp
public async Task JoinLobby(string lobbyId)   // Groups.AddToGroupAsync(ConnectionId, "lobby_" + lobbyId)
public async Task LeaveLobby(string lobbyId)  // Groups.RemoveFromGroupAsync(...)
```
The `POST /lobbies/{id}/start` controller action sends `gameStarted: { gameId }` to the lobby group (`"lobby_{lobbyId}"`). No new hub file needed.

The JWT token from create/join lobby is used to authenticate the SignalR connection.

The `Program.cs` SignalR JWT middleware already checks `/hubs/game` — extend it to also accept tokens for the same hub (no path change needed since it's the same `GameHub`).

#### Wiring
- `Doko.Application/ServiceCollectionExtensions.cs` — register `ICreateLobbyHandler`, `IJoinLobbyHandler`
- `Doko.Infrastructure/ServiceCollectionExtensions.cs` — register `ILobbyRepository` as singleton
- `Doko.Api/Extensions/ServiceCollectionExtensions.cs` — register `ITokenService`

---

### Frontend

#### App state machine (`App.tsx`)
```
type AppView =
  | { kind: 'home' }
  | { kind: 'hot-seat' }
  | { kind: 'lobby'; lobbyId: string; token: string; playerId: number; isHost: boolean }
  | { kind: 'game'; tokens: string[]; gameId: string; myPlayerId: number }
```

On mount:
1. Check `sessionStorage` for `dokoLobbySession` (JSON with `lobbyId`, `token`, `playerId`, `isHost`)
2. Check `?lobby={id}` query param
3. If query param matches stored session → restore lobby view
4. If query param but no session → navigate to lobby auto-join flow (call `POST /lobbies/{id}/join`)
5. Otherwise → show `'home'`

When navigating home→lobby, push `?lobby={lobbyId}` to URL via `window.history.pushState`.
When game starts, clear the query param.

#### New components

**`src/components/LandingPage/LandingPage.tsx`**
- Full-screen layout, centered, card-style
- "Lobby erstellen" button (primary, large touch target ≥56px)
- "Testspiel starten" button (secondary/muted, only rendered when `import.meta.env.DEV`)
- Mobile-first: stacked vertically, large text

**`src/components/LobbyPage/LobbyPage.tsx`**
- Title: "Lobby"
- Share section: URL display + copy-to-clipboard button (shows "Kopiert!" feedback for 2s)
- Player list: 4 rows — filled slots show "Spieler N" / "Du (Spieler N)", empty show "Wartet…"
- "Spiel starten" button: only shown to host (`isHost`), disabled until `playerCount === 4`
- Status text: "3 von 4 Spielern beigetreten" etc.
- Mobile-first: full width, readable on phone

**`src/hooks/useLobby.ts`**
- Creates/joins lobby (API calls)
- Connects SignalR with the lobby token
- Calls `hub.invoke('JoinLobby', lobbyId)` on connect
- Subscribes to `playerJoined` → updates player count
- Subscribes to `gameStarted` → triggers transition to game view (provides `gameId`)
- Stores/restores session from `sessionStorage`

#### Hot-seat stays unchanged
`useHotSeat` and `GameBoard` are untouched. The only change to App.tsx is wrapping them in the `'hot-seat'` branch of the state machine.

---

### Files to create
- `Code/backend/Doko.Domain/Lobby/LobbyId.cs`
- `Code/backend/Doko.Domain/Lobby/LobbyState.cs`
- `Code/backend/Doko.Application/Lobbies/ILobbyRepository.cs`
- `Code/backend/Doko.Application/Lobbies/Handlers/CreateLobbyHandler.cs`
- `Code/backend/Doko.Application/Lobbies/Handlers/JoinLobbyHandler.cs`
- `Code/backend/Doko.Application/Lobbies/Queries/LobbyView.cs`
- `Code/backend/Doko.Infrastructure/Repositories/InMemoryLobbyRepository.cs`
- `Code/backend/Doko.Api/Controllers/LobbiesController.cs`
- `Code/backend/Doko.Api/Services/ITokenService.cs`
- `Code/backend/Doko.Api/Services/JwtTokenService.cs`
- `Code/frontend/src/components/LandingPage/LandingPage.tsx`
- `Code/frontend/src/components/LobbyPage/LobbyPage.tsx`
- `Code/frontend/src/hooks/useLobby.ts`
- `Code/frontend/src/api/lobby.ts`

### Files to modify
- `Code/backend/Doko.Application/ServiceCollectionExtensions.cs` — register lobby handlers
- `Code/backend/Doko.Infrastructure/ServiceCollectionExtensions.cs` — register `ILobbyRepository`
- `Code/backend/Doko.Api/Extensions/ServiceCollectionExtensions.cs` — register `ITokenService`
- `Code/backend/Doko.Api/Controllers/AuthController.cs` — use `ITokenService`
- `Code/backend/Doko.Api/Hubs/GameHub.cs` — add `JoinLobby`/`LeaveLobby`
- `Code/backend/Doko.Api/Program.cs` — extend SignalR JWT middleware to lobby hub path
- `Code/frontend/src/App.tsx` — state machine, URL handling
