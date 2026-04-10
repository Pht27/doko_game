The game info cannot be a whole bar on top, that takes too much space. It should be a small hovering box on the top left (and not space on the x axis so the one player label can be on the same height). Similarily, the PlayerSwitcher should be a hovering box (layed out vertically) in the top right corner. The Announcement button should be on like 20% of screen height on the bottom left instead of above the cards.

## Plan

### What changes
1. **Remove the top bar** (`flex items-center justify-between px-4 py-2 bg-black/30`) — no more dedicated header row
2. **GameInfo** → `absolute` positioned, `top-2 left-2`, small floating box with semi-transparent background; frees up vertical space so top PlayerLabel can be at the same height
3. **PlayerSwitcher** → `absolute` positioned, `top-2 right-2`, change from horizontal (`flex`) to vertical (`flex-col`) layout
4. **AnnouncementButton** → `absolute` positioned at `bottom-[20%] left-4` (20% from bottom of game container), removed from the normal flex flow

### Files affected
- `src/frontend/src/App.tsx` — restructure top bar, move components to absolute positioning
- `src/frontend/src/styles/GameInfo.css` — add floating box styles (bg, padding, rounded)
- `src/frontend/src/styles/PlayerSwitcher.css` — change to flex-col
- `src/frontend/src/styles/AnnouncementButton.css` — no change needed (positioning handled in App.tsx)

### Trade-offs
- Using `absolute` on outer div (`w-full h-full relative`) so both GameInfo and PlayerSwitcher float over the full screen
- AnnouncementButton uses `absolute bottom-[20%] left-4` within the game area which is already `relative`
