## Goal

Replace the current combined "Ansagen: N" value component with per-announcement breakdown,
so the result screen can show individual announcement lines merged with their corresponding
value components:

```
Gewonnen (angesagt)  +2    ← winning party announced Win → base +1 + announcement +1
Keine 90             +1    ← threshold crossed, but not announced
Keine 90 (angesagt)  +2    ← threshold crossed AND announced → +1 + +1
```

Instead of:
```
Gewonnen    +1
Ansagen     +2
```

---

## Scoring background

Each announcement adds exactly +1 to the game value, regardless of which party makes it:
- Re/Kontra (AnnouncementType.Win) → +1 per party that announces it
- Keine90 / Keine60 / Keine30 / Schwarz → +1 per party that announces it (must also be achieved)

The current `GameScorer` emits a single `{label: "Ansagen", value: N}` component covering all N
announcements in one line. The individual announcements are present in `state.Announcements`
but not forwarded to the result DTO.

---

## Implementation Plan

### Backend: Expose per-announcement data in GameResultDto

**`Doko.Api/DTOs/Responses/GameResultDto.cs`**
- Add `IReadOnlyList<AnnouncementRecordDto> AnnouncementRecords`
- `record AnnouncementRecordDto(string Party, string Type)` — e.g. `{Party: "Re", Type: "Win"}` or `{Party: "Kontra", Type: "Keine90"}`

**`Doko.Domain/Scoring/GameScorer.cs`**
- Remove `components.Add(new("Ansagen", announcementsCount))`
- Instead, collect `AnnouncementRecord(party, type)` entries from `state.Announcements`,
  resolving each player's party via `state.PartyResolver`
- Still add +1 to `gameValue` per announcement (scoring is unchanged)

**`Doko.Domain/Scoring/GameResult.cs`**
- Add `IReadOnlyList<AnnouncementRecord> AnnouncementRecords` (domain type: Party + AnnouncementType)

**`Doko.Api/Mapping/DtoMapper.cs`**
- Map `GameResult.AnnouncementRecords` → `IReadOnlyList<AnnouncementRecordDto>` using
  `party.ToString()` and the same "Win → Re/Kontra" alias logic as the announcement button labels

**`Doko.Domain/Lobby/LobbyState.cs`**
- No change — `GameHistory` already stores `GameResult`; once the domain model has
  `AnnouncementRecords` they are automatically included

### Frontend: API types

**`Code/frontend/src/types/api.ts`**
- Add `interface AnnouncementRecordDto { party: string; type: string }`
- Add `announcementRecords?: AnnouncementRecordDto[]` to `GameResultDto`

### Frontend: ResultDisplay — merge announcements with value components

In `ResultDisplay.tsx`, when rendering the component table:

1. Build a set of which announcement types were made by which party:
   ```ts
   const announced = new Set(result.announcementRecords?.map(r => `${r.party}:${r.type}`) ?? [])
   ```

2. For each value component, check for a matching announcement from the correct party:
   - `"Gewonnen"` matches `Win` by the **winning party**
   - `"Keine 90"` matches `Keine90` by either party (each party's announcement independently)
   - Same for `Keine60`, `Keine30`, `Schwarz`
   - `"Gegen die Alten"` has no corresponding announcement type

3. Display rule for a matched component:
   - Label becomes `"Gewonnen (angesagt)"` / `"Keine 90 (angesagt)"`
   - Displayed value becomes `component.value + matchedCount` (usually base +1, total +2)
   - The sign adaptation (winning/losing player perspective) applies on top, as for all other rows

4. Remaining unmatched announcement records (e.g. losing party also announced Win):
   - Show as separate lines below: `"Kontra angesagt +1"` (sign-adapted as usual)

### Translations

Add to `translations.ts`:
- `announcedSuffix: '(angesagt)'` — appended to merged component labels
- For unmatched records, reuse existing `announcementLabels` + a `"... angesagt"` suffix
