# Active Lobbies / Concurrent Games

## Current State

The backend already supports multiple concurrent games — both `InMemoryGameRepository` and `InMemoryLobbyRepository` use `ConcurrentDictionary` keyed by their respective IDs. Multiple games can run simultaneously without any issue.

Three things are missing:
1. Started lobbies are hidden from the lobby list (`GetAllAsync` filters them out).
2. "Neues Spiel" on the result screen always sends multiplayer players back to the home screen instead of the lobby.
3. `StartLobbyGame` blocks restart with "lobby_already_started" even after a game has ended.

## Implementation Plan

### Backend

**`Doko.Domain/Lobby/LobbyState.cs`**
- Add `GameId? ActiveGameId` property.
- Change `MarkStarted()` to `MarkStarted(GameId gameId)` — records which game is active.
- Add `MarkGameFinished()` — resets `IsStarted = false` and clears `ActiveGameId`.

**`Doko.Api/DTOs/Responses/LobbyResponses.cs`**
- Add `IsStarted` to `LobbyListItemResponse`.

**`Doko.Infrastructure/Repositories/InMemoryLobbyRepository.cs`**
- `GetAllAsync`: return all lobbies (remove the `!l.IsStarted` filter) so in-progress lobbies appear in the browser.

**`Doko.Api/Controllers/LobbiesController.cs`**
- `ListLobbies`: pass `IsStarted` through to the DTO.
- `StartLobbyGame`: if `lobby.IsStarted`, call `lobby.MarkGameFinished()` and proceed (allow restart). Pass `gameId` to `MarkStarted(gameId)`.

### Frontend

**`Code/frontend/src/api/lobby.ts`**
- Add `isStarted: boolean` to `LobbyListItemResponse`.

**`Code/frontend/src/App.tsx`**
- Add `lobbySession?: LobbySession` to the `{ kind: 'game' }` view variant.
- `handleGameStarted`: store the full `session` in the game view state.
- `onNewGame` for multiplayer: restore the session to sessionStorage, then navigate to `multiplayer-browser` with `selectedLobbyId` set to the lobby from the stored session.

**`Code/frontend/src/components/MultiplayerBrowserPage/MultiplayerBrowserPage.tsx`**
- Show a small "Spiel läuft" badge on lobby list entries where `isStarted === true`.

## Files affected

- `Code/backend/Doko.Domain/Lobby/LobbyState.cs`
- `Code/backend/Doko.Api/DTOs/Responses/LobbyResponses.cs`
- `Code/backend/Doko.Infrastructure/Repositories/InMemoryLobbyRepository.cs`
- `Code/backend/Doko.Api/Controllers/LobbiesController.cs`
- `Code/frontend/src/api/lobby.ts`
- `Code/frontend/src/App.tsx`
- `Code/frontend/src/components/MultiplayerBrowserPage/MultiplayerBrowserPage.tsx`
