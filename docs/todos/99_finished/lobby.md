# Bug: Lobby view doesn't disappear for viewers when lobby is deleted

When a lobby is deleted (last player left), a player who was watching the lobby (had it open but hadn't joined) doesn't see the lobby disappear.

## Root Cause

`useLobby` in `hooks/useLobby.ts` only connects to SignalR when `session !== null` (line 71). Viewers have no session, so they never receive the `lobbyClosed` SignalR event. They only get the initial seat state via `getLobby()` once and are never updated.

## Fix

Add a polling loop in `useLobby` for viewers (no session). Every 3 seconds, call `getLobby()`. If it throws (HTTP 404), the lobby is gone — set `lobbyClosed = true`. This mirrors the existing polling in `MultiplayerBrowserPage` for the lobby list.

### Files affected

- `Code/frontend/src/hooks/useLobby.ts` — add a polling effect that only runs when `session` is null

### Implementation

In the second `useEffect` (the SignalR one), early return when no session is already in place. Add a new `useEffect` below it:

```typescript
// Viewer polling: detect lobby deletion when no session (no SignalR connection)
useEffect(() => {
  if (session) return;
  const id = setInterval(async () => {
    try {
      await getLobby(lobbyId);
    } catch {
      setLobbyClosed(true);
    }
  }, 3000);
  return () => clearInterval(id);
}, [lobbyId, session]);
```

No backend changes needed. The `lobbyClosed` state is already handled in `LobbyDetailView` — it calls `onLobbyClosed()` which navigates back.
