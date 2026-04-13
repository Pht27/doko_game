

## Plan

### Task 1: Sonderkarte notification at player label

When a `SonderkarteTriggered` SignalR event arrives (`{ player, type }`), briefly show a badge
at the triggering player's `PlayerLabel`. Auto-clears after 3 s. Own player's label doesn't exist
(they use hand/own-party UI), so no special "not for own" filtering needed.

**Files affected:**
- `useGameState.ts` — capture event, expose `sonderkarteNotification: {player, type} | null`
- `App.tsx` — thread notification down to `GameBoard`
- `GameBoard.tsx` — pass notification per-player to `PlayerLabel`
- `PlayerLabel.tsx` — render brief badge when present
- `PlayerLabel.css` — animation/style for badge
- `translations.ts` — Sonderkarte display names

### Task 2: Show announcements (Ansagen) at player labels

When a player announces (Ansage), their announcement and party must be shown on their label,
even in silent solos.

`PlayerPublicStateDto` currently has no announcement info and `KnownParty` is always null
(never set by any modification). Fix both:

**Backend:**
- `PlayerPublicState.cs` — add `string? HighestAnnouncement`
- `GameQueryService.cs` — for each other player, find the max `AnnouncementType` in
  `state.Announcements`; also expose correct `KnownParty` for players who have announced
  (via party resolver)
- `PlayerPublicStateDto.cs` — add `string? HighestAnnouncement`
- `DtoMapper.cs` — pass through

**Frontend:**
- `types/api.ts` — add field to interface
- `PlayerLabel.tsx` — render announcement badge when present
- `PlayerLabel.css` — style

### Trade-offs
- `KnownParty` was never populated before; fixing it here means party dot finally works for
  announcing players. In non-announcing silent solos it stays null (intended by the todo).
- Backend sends "Re"/"Kontra" for `Win` type (party-resolved), raw enum name for others.
  Frontend uses existing `announcementLabel()` to display them.
