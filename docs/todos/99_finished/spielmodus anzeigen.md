# Spielmodus anzeigen

spielmodus anzeigen und in (nicht stillen) soli direkt die parteien anzeigen

## Clarification

- **Stille Soli** = Stille Hochzeit + Kontrasolo (not yet implemented in codebase)
- **Nicht stille Soli** = all current solos (Farbsolo, Damen, Buben, Fleischloses, Knochenloses, SchlankerMartin) → parties visible immediately
- **Hochzeit** → parties visible once partner is found (after Findungsstich)

## Implementation Plan

### What changes

**1. Show game mode (Spielmodus)**
- Add `ActiveGameMode: string?` to `PlayerGameView` and `PlayerGameViewResponse`
- Value = `state.ActiveReservation?.Priority.ToString()` (null = Normalspiel)
- Display in `GameInfo` (top-right box): replaces phase label during Playing phase; other phases show a translated German phase name

**2. Reveal parties in non-silent solos, Armut, and resolved Hochzeit**
- In `GameQueryService`, compute:
  ```csharp
  bool revealParties =
      state.ActiveReservation is not null
      && (state.ActiveReservation.IsSolo || state.PartyResolver.IsFullyResolved(state));
  ```
- Covers: all solos (IsSolo=true), Armut (IsFullyResolved always true), Hochzeit after partner found / Stille Hochzeit (IsFullyResolved=true); Hochzeit before partner found and Normalspiel stay hidden
- When `revealParties == true`, set `knownParty` for all other players unconditionally (without needing an announcement)
- Own party (`ownParty`) already works correctly via `PartyResolver.ResolveParty`

### Files affected

**Backend:**
- `Doko.Application/Games/Queries/PlayerGameView.cs` — add `ActiveGameMode` property
- `Doko.Api/DTOs/Responses/PlayerGameViewResponse.cs` — add `ActiveGameMode` property
- `Doko.Application/Games/GameQueryService.cs` — compute `revealParties` and `activeGameMode`, pass to view
- `Doko.Api/Mapping/DtoMapper.cs` — thread `ActiveGameMode` through `ToResponse`

**Frontend:**
- `src/frontend/src/types/api.ts` — add `activeGameMode: string | null`
- `src/frontend/src/translations.ts` — add `gameModeLabel(mode)` function + German phase label map
- `src/frontend/src/components/shared/GameInfo.tsx` — accept `gameMode` prop; show mode or translated phase
- `src/frontend/src/components/GameBoard/GameBoard.tsx` — pass `activeGameMode` to `GameInfo`

### Non-obvious decisions

- Stille Hochzeit (detected by `HochzeitPartyResolver.IsFullyResolved` returning true without a partner) falls back to Kontra for all non-Hochzeit players — already handled by the resolver; parties are revealed at that point too (which is correct, since it's effectively a solo at that point)
- `PlayerLabel` already renders the party dot when `knownParty` is non-null → no component changes needed there
- During reservation phases the active game mode is null (not yet decided) → show translated phase name instead
