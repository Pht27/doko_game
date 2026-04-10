# Announcements: Not Turn-Bound

Announcements in our play style are not turn-bound. They have the deadline criteria as
already implemented (up until before the second card of the second trick with the
extensions), but apart from that, anyone can announce anything regardless of whether it
is their turn or not.

## Analysis

**Backend is already correct.** `MakeAnnouncementUseCase` does not check `CurrentTurn` —
it only validates phase and `AnnouncementRules.CanAnnounce`, which has no turn check
either. `GameQueryService` computes `legalAnnouncements` independently of `isMyTurn`
(lines 48-52).

**The only gating is in the frontend.** `App.tsx:169` wraps the `<AnnouncementButton>`
with `view?.isMyTurn &&`, which hides it when it's not the player's turn.

## Changes Required

### 1. Frontend — `frontend/src/App.tsx` (line 169)

Remove the `view?.isMyTurn &&` guard from the `AnnouncementButton`:

```diff
- {view?.isMyTurn && (
-   <AnnouncementButton
-     legalAnnouncements={view.legalAnnouncements}
-     onAnnounce={handleAnnouncement}
-   />
- )}
+ <AnnouncementButton
+   legalAnnouncements={view?.legalAnnouncements ?? []}
+   onAnnounce={handleAnnouncement}
+ />
```

`AnnouncementButton` already returns `null` when `legalAnnouncements` is empty, so the
button only appears when the player actually has a legal announcement available.

### 2. No backend changes needed

The use case and domain rules are already turn-agnostic for announcements.

## Files Touched

- `frontend/src/App.tsx` — remove turn guard on announcement button
