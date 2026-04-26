please use more components in the result screen. just define subcomponents and put them in the same folder if you feel like that is somehting that could be separated into its own component.

i have a more thorough design in mind: we will work with a match history. that means we also have to persist the game results in the lobby.

the result screen is going to be more wide. top level, two columns:
1. lobby display
2. result display + button

## Lobby display
the lobby display displays much information about the lobby with a kind of "match history". it works like a table: we have a top row for the player names (with the own name highlighted in blue like it is right now) and below rows. the rows have a game number and then in the column of each player the points they scored / lost that game. the game that was just finished is selected (see explaination below). when clicking on a game row, the result display displays the result of that game. on the bottom there is a standings row with the current standings ie the sum of the games before. (it is not clickable)

here is a rough sketch

| game  | player1   | >player2<   | player3   | player4 |
| --    | --        | --        | --        | --        |
| 1     | -2        | -2        | +2        | +2        |
| 2     | +9        | -3        | -3        | -3        |
...
| --    | --        | --        | --        | --        |
|       | 15        | 6        | -11        | -10        |

i dont want it to look like a rigid table, so i would make the table bars very subtle or not display at all i think? please give me a design idea

but please make the columns not flexible ie they should all have the same width regardless of player name 


## second column
the second column displays the resultdisplay and below the ready/new game button.

### resultDisplay
the result display is divided into 3 columns. the columns dont need a header i just give them names here for clarity

#### first column: stiche and augen
"Re/Kontra gewinnt!" wuth below an info how many Augen in how many Stiche each party made (make the winner party subtly stand out here). maybe we can use a shade of the re and contra colors we use in game? lets try that 

#### second column: extrapunkte
each extrapunkt gets shown with the player name who scored it and the corresponding point(s) (in case someone made 2 fischaugen for example). maybe we can color the player names like above with the re / kontra colors to somehow indicate their party without making it too cluttered.

#### third column: total game value
a display of how the game value gets computed (like it is now) with the extrapoints summed already as one row (we already explain them in column two, no need to disect them again). mind here that you have to display it positively or negatively depending on whihc party the player was. and a total sum spielwert for the individual player (ie plus or minus!)

### ready button

the ready button does not need to change much, i just want the display of how many players are ready to be inside the button on the right. make sure the "Bereit" label stays centered and display the no of ready players with a number indicator ("2/4") and next to that i want (only) a small icon: 👤👤👤

---

## Implementation Plan

### Overview

The redesign requires:
1. **Backend**: Store per-game results (not just cumulative standings) in `LobbyState`, expose them in `GameResultDto` so the frontend can render a clickable match history.
2. **Frontend**: Wide 2-column result screen — left column is a match history table, right column is a 3-sub-column result detail + new ready button.

### Backend Changes

#### `LobbyState.cs` (`Doko.Domain/Lobby/LobbyState.cs`)
- Add `_gameHistory: List<(GameResult Result, int[] NetPoints)>`
- Expose `IReadOnlyList<(GameResult Result, int[] NetPoints)> GameHistory`
- Add `AddGameRecord(GameResult result, int[] netPoints)` — call this instead of (or alongside) `UpdateStandings` for real completed games. Schmeißen does NOT add a record.

#### `GameResultDto.cs` (`Doko.Api/DTOs/Responses/GameResultDto.cs`)
- Add field: `IReadOnlyList<GameResultDto>? MatchHistory = null`
  - Self-referential: the current result contains the full history of prior games. Each history entry has `MatchHistory = null`.

#### `DtoMapper.cs` (`Doko.Api/Mapping/DtoMapper.cs`)
- Update `ToDto(...)` signature to accept `IReadOnlyList<GameResultDto>? matchHistory = null`
- Pass it through to the record constructor

#### `GamesController.cs` (`Doko.Api/Controllers/GamesController.cs`)
- In `HandleGameFinishedAsync`: after `lobby.UpdateStandings(netPoints)`, also call `lobby.AddGameRecord(finished.Result, netPoints)`
- Build `historyDtos` from `lobby.GameHistory` using `DtoMapper.ToDto` for each past entry (without their own history)
- Pass `historyDtos` to the final `DtoMapper.ToDto` for the current result

### Frontend Changes

#### `types/api.ts`
- Add `matchHistory?: GameResultDto[]` to `GameResultDto`

#### New `LobbyHistory.tsx` (`components/ResultScreen/LobbyHistory.tsx`)
- Props: `result: GameResultDto, mySeat?: number, selectedGame: number, onSelectGame: (i: number) => void`
- Renders: fixed-width column table with player header row, per-game rows (net points colored +/−), selected row highlighted, bottom standings row
- All columns equal fixed width; player name columns abbreviated if needed ("S1"–"S4")
- Own seat column header highlighted (indigo)
- No visible table borders — subtle bg shading on selected row, divider before standings

#### `ResultDisplay.tsx` (redesigned)
- Props: `result: GameResultDto, mySeat?: number`
- Three internal columns (no headers shown):
  1. **Stiche/Augen**: Winner banner with Re/Kontra party color shade, then two rows: Re Augen count / Kontra Augen count, winner row subtly bolded or tinted
  2. **Extrapunkte**: List of `allAwards` — for each: player label (`S{seat}`) colored blue (Re) or red (Kontra) based on derived party, award type, and point delta. Party derived from `netPointsPerSeat` + `winner`.
  3. **Spielwert**: Computation breakdown from `valueComponents`, then extrapunkte as one summed line, then solo factor if > 1, then total for *this player* (positive or negative, derived from `netPointsPerSeat[mySeat]`). Show a total Spielwert row clearly.

#### `ResultScreen.tsx` (restructured)
- `max-w-3xl` wide overlay (instead of `max-w-sm`)
- Two outer columns: left = `LobbyHistory`, right = detail + button
- State: `selectedGame: number` (default = last game index, i.e., current game)
- The result shown on the right: if `selectedGame` is the current game → show `result`; if a past game → show `result.matchHistory[selectedGame]`
- Ready button redesign: full-width button, "Bereit" centered, right-aligned `"2/4 👤"` indicator inside the button (using flex justify-between or similar)

#### `GeschmissenDisplay.tsx` (adapted)
- Keep as a right-column-only display (no augen, no extrapunkte columns)
- Just shows the Schmeißen message; the left column (`LobbyHistory`) is still rendered by `ResultScreen`
- The history table for Geschmissen works the same — the history entries are all prior real games (no new entry for Schmeißen itself)

#### `ResultScreen.css`
- Add classes for wide layout, fixed-column history table, 3-sub-column result detail, new ready button style

#### `translations.ts`
- Add any new label keys needed (e.g., per-column headers, Stiche label, etc.)

### Non-goals / Trade-offs
- Hot-seat (solo test) mode: no lobby → no match history → LobbyHistory column not rendered; ResultScreen falls back gracefully.
- Page refresh mid-session: history is stored on backend, so it survives refreshes.
- Schmeißen adds no row to the history table.
