some changes: when the bereit button is cllicked and it duisplays zurückziehen it ofc should still display the current no of bereit playesrs.

lets put a bar / line between the result display and the match history

make the bereit Label on the button centered and not change due to the no of playres ready!! not flexible!

i liked the all caps asthetic, lets try that again

in the match history, the game list must be scrollable of course but the standings and players rows need to stay fixed. same thing goes for the calculation table of the game value (that can have like 10 components in some games).

then lets give the lobby history a header/title ("Match History"). also, the resultsscreen should always have the same dimensions, maybe we can fix it by defining it through the distance to the screen borders. then, in the match history, the players row should always be as high as possible below the title and the standings row as far down as possible (ie leave place in the game rows if theyre not all filled yet).

the results display should look like this:

----------------------------------
<GameMode>: <WinningParty> gewinnt
Re <AugenRe> (<AnzahlSticheRe>) | <AugenKontra> (<AnzahlSticheKintra>) Kontra
<Ergebniskomponententabelle>
<ReadyButton>
---------------------------------

wobei die Ergebniskomponententabelle alle einzelnen Punkte (inkl. Extrapunkte) listet aber *schon individuell an den Spieler angepasst*!! Das heißt, wenn Kontra gewonnen hat, sieht der Re Spieler
Gewonnen -1
gegen die Alten -1
keine 90 -1
---
Fischauge (<ein re spieler>) +1
Karlchen (<ein Kontraspieler>) -1
---
Insgesamt -3


Ansagen möchte ich dann so displayen (vlt muss man dafür nochmehr infos aus dem backend holen). Angenommen Kontra hat gewonnen mit keine 90 und Kontra (gewinnen) angesagt

Gewonnen (angesagt) +2
keine 90            +1
...

Falls man die individuelle Berechnung der komponenten für einen Spieler als Funktion auslagern will, kann das meinetwegen auch im Frontend passieren, es geht ja nur um die Anzeige. Wenn es allerdings eine Berechtigung im Backend hat, ist auch okay dort.

---

## Implementation Plan

### Backend: Add ReStiche, KontraStiche, GameMode to result DTOs

The result display needs trick counts per party and the game mode (for the header line). These aren't in the current DTO.

**`Doko.Domain/Scoring/GameResult.cs`**
- Add `int ReStiche`, `int KontraStiche`

**`Doko.Domain/Scoring/GameScorer.cs`**
- Count trick wins per party alongside the existing Augen summing loop

**`Doko.Application/Games/Results/GameFinishedResult.cs`**
- Add `string? GameMode` (null = Normalspiel)

**`Doko.Application/Games/Handlers/FinishGameHandler.cs`**
- Set `GameMode = state.ActiveReservation?.Priority.ToString()`

**`Doko.Api/DTOs/Responses/GameResultDto.cs`**
- Add `int ReStiche`, `int KontraStiche`, `string? GameMode`

**`Doko.Domain/Lobby/LobbyState.cs`**
- Change `_gameHistory` type from `List<(GameResult, int[])>` to `List<(GameResult, string? GameMode, int[])>`
- `AddGameRecord()` gains a `string? gameMode` parameter

**`Doko.Api/Controllers/GamesController.cs`**
- Pass `finished.GameMode` to `lobby.AddGameRecord()`
- `BuildMatchHistory()` extracts game mode per entry and passes it to `DtoMapper.ToDto()`

**`Doko.Api/Mapping/DtoMapper.cs`**
- Map `ReStiche`/`KontraStiche` from `GameResult`
- Add optional `string? gameMode = null` parameter to `ToDto()` and pass it through

### Frontend: API types
**`Code/frontend/src/types/api.ts`**
- Add `reStiche?: number`, `kontraStiche?: number`, `gameMode?: string` to `GameResultDto`

### Frontend: ResultDisplay — redesign to vertical single-column

Replace the 3-column grid with a vertical list:

```
NORMALSPIEL: RE GEWINNT          ← uppercase, winner highlighted
Re 87 (4) | 153 (8) Kontra       ← Augen + stiche per party
──────────────────────────────────
Gewonnen          -1             ← value components, sign adapted to mySeat's party
Gegen die Alten   -1
Keine 90          -1
Ansagen           -2
──────────────────────────────────  (only if allAwards exist)
Fischauge (S1)    +1             ← extrapunkte with per-player attribution + adapted sign
Karlchen (S3)     -1
──────────────────────────────────
Insgesamt         -3             ← = netPointsPerSeat[mySeat]; fallback: totalScore
```

Player-specific sign logic (done in frontend):
- `myParty`: derived from `netPointsPerSeat[mySeat] > 0 ? winner : opponent`
- Value component sign: `+` if on winning side, `-` if losing
- Extrapunkt sign: `+` if benefitting player is on my party, `-` if opponent
- If `mySeat` is undefined (spectator): show unsigned values

Note: Announcement breakdown ("Gewonnen (angesagt) +2") is deferred to `docs/todos/1_bugfixes/ansagen_points.md`. The current "Ansagen: N" combined component stays for now.

### Frontend: LobbyHistory — title + fixed-height layout

```
MATCH HISTORY                    ← new title
─────────────────────────────────
S1    S2    S3    S4             ← header row (stuck at top)
─────────────────────────────────
+2    -2    +2    -2             ← game rows (flex-1, overflow-y-auto)
-1    +1    -1    +1
                                 ← empty space fills if fewer games than panel height
─────────────────────────────────
+1    -1    +1    -1             ← standings row (stuck at bottom)
─────────────────────────────────
```

Implement with `flex flex-col h-full`: header fixed, `flex-1 overflow-y-auto` game rows, standings at bottom.

### Frontend: ResultScreen — fixed dimensions + button fixes

**Dimensions**: Change `.result-screen-wide` to fill the viewport with a fixed inset (e.g., `h-[calc(100vh-24px)] w-full max-w-3xl overflow-hidden`). Inner columns use `h-full flex flex-col`.

**Bereit button**: Always show vote count `{voteCount}/4 👤` regardless of `hasVoted` state. When `hasVoted`, show it greyed out next to "Zurückziehen". Layout: fixed-width indicator slot so label stays centered in both states.

**Divider**: Add a vertical `border-l border-white/15` between left and right columns.

**All-caps**: Apply `uppercase tracking-wider` to winner banner, match history title, seat headers.

### Translations
- Add `matchHistory: 'Match History'` and `insgesamt: 'Insgesamt'` to `translations.ts`