
I dont like the handling of Schmeißen result screen. The schmeißen game still counts as game -> it should not just be frontend logic. If we store the games played in the lobby (not implemented yet), the geschmissen games should still be persisted, of course with some kind of extra logic/flag, but still. Maybe we need to think about if there is some kind of special game result called "Geschmissen"...? Then we can make the result screen look at the result and check if it was "Geschmissen" etc. And the logic for not advancing the Rauskommer should not be decided in the frontend. I think, if possible, the lobby should decide whether to advance using the last GameResult. Then we can also streamline this to not advance on Solo and Armut.

And in general the layout of the result screen: I think we need to make it more components. The geschmissen screen is fine, but Im thinking of making a subcomponents layout in the resultscreen component: You always have a ResultDisplay/GeschmissenDisplay and the NewGameButton (the latter functions the same whether it was geschmissen or not, as that logic should be handled in the backend as mentioned above). The Resultscreen decides whether to display the Geschmissen or Result Display by a the game result it gets. The geschmissen screen can stay as it is.

The result display however I want like something like this: It should be a two column layout. Left column is the current lobby standings with the current points for each player (after applying the current result ofc) and a green or red number which displays the points the respective player won / lost this game. On the right column should be the info about the current round but each player should only be displayed the points they get rewarded / subtracted. It should still show the Augen and GameValue etc. like before.

---

## Implementation Plan

### Summary

Unify the Geschmissen flow with the normal result flow by producing a proper `GameResultDto` (with `isGeschmissen: true`) when Schmeißen occurs. The backend decides Rauskommer advancement (already controlled by the `_advanceRauskommer` flag in `LobbyState`). A single voting endpoint handles both cases. The frontend's `ResultScreen` renders either `GeschmissenDisplay` or `ResultDisplay` (2-column layout) based on the result flag.

---

### Backend Changes

**1. `GameResultDto.cs`** — Add `bool IsGeschmissen` field (default `false` for normal games).

**2. `DtoMapper.cs`** — Add optional `isGeschmissen` parameter to `ToDto(GameResult, ...)`, pass it through.

**3. `GamesController.cs`** — In `MakeReservation`, after receiving a geschmissen result:
- Fetch lobby by game ID
- Call `lobby.SetAdvanceRauskommer(false)` and save
- Build a `GameResultDto` with `IsGeschmissen: true`, zeroed scores, current lobby standings
- Fire `gameFinished` SignalR event to the game group

**4. `LobbiesController.cs`** — Remove `VoteNewGameGeschmissen` endpoint. The regular `VoteNewGame` already calls `lobby.AdvanceRauskommerIfRequired()`, which reads the stored flag set in step 3.

---

### Frontend Changes

**5. `types/api.ts`** — Add `isGeschmissen: boolean` to `GameResultDto`.

**6. `api/lobby.ts`** — Remove `voteNewGameGeschmissen` function.

**7. New component `ResultScreen/GeschmissenDisplay.tsx`** — Extracted from old `GeschmissenResultScreen`: title, subtitle, standings table (no colored delta since net points are all 0).

**8. New component `ResultScreen/ResultDisplay.tsx`** — Two-column layout:
- **Left column**: Lobby standings (cumulative total per player) with colored (+/-) net point delta for this game
- **Right column**: Game details — Winner title, Augen, Spielwert breakdown (valueComponents + gameValue), Feigheit banner, Gesamtergebnis (if different), Zusatzpunkte

**9. `ResultScreen/ResultScreen.tsx`** refactor — Container that:
- Receives a `result: GameResultDto` and `multiplayerNewGame` props
- Renders `GeschmissenDisplay` or `ResultDisplay` based on `result.isGeschmissen`
- Renders `NewGameButton` (voting logic) below either display

**10. `ResultScreen.css`** — Add 2-column grid styles for `ResultDisplay`.

**11. `GameBoard.tsx`** — Remove `view?.phase === 'Geschmissen'` check and `GeschmissenResultScreen` import; remove `multiplayerGeschmissenNewGame` prop.

**12. `App.tsx`** — Remove `multiplayerGeschmissenNewGame` prop and `voteNewGameGeschmissen` import.

**13. Delete** `GeschmissenResultScreen/GeschmissenResultScreen.tsx` (functionality folded into `ResultScreen`).

---

### Files Affected

| File | Change |
|---|---|
| `Doko.Api/DTOs/Responses/GameResultDto.cs` | Add `IsGeschmissen` |
| `Doko.Api/Mapping/DtoMapper.cs` | Pass `IsGeschmissen` through |
| `Doko.Api/Controllers/GamesController.cs` | Fire gameFinished on Geschmissen |
| `Doko.Api/Controllers/LobbiesController.cs` | Remove geschmissen endpoint |
| `frontend/src/types/api.ts` | Add `isGeschmissen` to DTO |
| `frontend/src/api/lobby.ts` | Remove `voteNewGameGeschmissen` |
| `frontend/src/components/ResultScreen/ResultScreen.tsx` | Refactor into container |
| `frontend/src/components/ResultScreen/GeschmissenDisplay.tsx` | New |
| `frontend/src/components/ResultScreen/ResultDisplay.tsx` | New, 2-column |
| `frontend/src/styles/ResultScreen.css` | 2-column grid styles |
| `frontend/src/components/GameBoard/GameBoard.tsx` | Remove Geschmissen phase check |
| `frontend/src/App.tsx` | Remove geschmissen props |
| `frontend/src/components/GeschmissenResultScreen/` | Delete |
