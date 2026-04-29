# Group phase-locked nullable fields into sub-records

> See: [domain_layer_architecture.md](../domain_layer_architecture.md) § 1c

> **Required by:** `big-refactors/domain-typed-state-per-phase` — do this first.

Without a full typed-state split, collapse the phase-locked nullables in `GameState` into sub-records (`ArmutState?`, `GenscherState?`, `SilentModeState?`). `Armut is null` becomes the explicit "not in Armut" signal. Reduces 30 properties to ~12 and makes the implicit state machine explicit. Prerequisite for or alternative to the big `big-refactors/domain-typed-state-per-phase` item.

---

## Scope

**In scope:**
- Introduce `ArmutState` record grouping the 4 Armut-phase fields (`Player`, `RichPlayer?`, `TransferCount`, `ReturnedTrump?`)
- Introduce `GenscherState` record grouping the 3 Genscher-phase fields (`TeamsChanged`, `PreRePlayers?`, `SavedAnnouncements?`)
- Replace those 7 individual properties on `GameState` with `ArmutState? Armut` and `GenscherState? Genscher`
- Update all read call sites across Domain, Application, and Api layers
- Update `Apply()` arms to mutate via sub-record `with` expressions

**Out of scope:**
- `SilentMode` — already a clean `SilentGameMode?` nullable record; no wrapper needed
- `HochzeitBecameForcedSolo` and `IsSchwarzesSau` — simple booleans, not multi-field nullable clusters
- `GameState.Create()` factory — none of the 7 fields are passed through it (tests always go via `Apply()`)
- Any logic changes; this is a pure structural rename/grouping

## Files To Touch

- `Code/backend/Doko.Domain/GameFlow/ArmutState.cs` *(new)* — `record ArmutState` with four properties
- `Code/backend/Doko.Domain/GameFlow/GenscherState.cs` *(new)* — `record GenscherState` with three properties
- `Code/backend/Doko.Domain/GameFlow/GameState.cs` — remove 7 fields, add `Armut?`/`Genscher?`; update `Apply()` arms for the 5 Armut mods and the Genscher mod
- `Code/backend/Doko.Application/Games/GameQueryService.cs` — `state.ArmutRichPlayer` → `state.Armut?.RichPlayer`, `state.ArmutTransferCount` → `state.Armut!.TransferCount`, `state.ArmutReturnedTrump` → `state.Armut?.ReturnedTrump`
- `Code/backend/Doko.Application/Games/Handlers/AcceptArmutHandler.cs` — `state.ArmutPlayer!.Value` → `state.Armut!.Player`
- `Code/backend/Doko.Application/Games/Handlers/ExchangeArmutCardsHandler.cs` — `state.ArmutRichPlayer` → `state.Armut!.RichPlayer`, `state.ArmutTransferCount` → `state.Armut.TransferCount`, `state.ArmutPlayer!.Value` → `state.Armut.Player`
- `Code/backend/Doko.Domain/Announcements/AnnouncementRules.cs` — `state.GenscherTeamsChanged` → `state.Genscher?.TeamsChanged ?? false`
- `Code/backend/Doko.Api/Mapping/DtoMapper.cs` — `view.ArmutReturnedTrump` → `view.ArmutReturnedTrump` (unchanged if PlayerGameView is the source; trace PlayerGameView.cs too)
- `Code/backend/Doko.Application/Games/Queries/PlayerGameView.cs` — `ArmutReturnedTrump` sourced from `state.Armut?.ReturnedTrump`
- `Code/tests/Doko.Domain.Tests/GameFlow/GameStateTests.cs` — `state.GenscherTeamsChanged` → `state.Genscher?.TeamsChanged ?? false`
- `Code/tests/Doko.Application.Tests/Games/Handlers/AcceptArmutHandlerTests.cs` — `state.ArmutRichPlayer` → `state.Armut?.RichPlayer`, `state.ArmutTransferCount` → `state.Armut?.TransferCount`

## Test Plan

- No new tests required — the change is pure structural; all behaviour is covered by existing tests
- Existing tests that read the grouped properties directly must be updated (see Files To Touch above); they will fail to compile until updated, which is the expected guard
- `GameStateTests.cs:141` (`Apply_Genscher_InKontraSolo_DoesNotSetGenscherTeamsChanged`) — update assertion to read `state.Genscher?.TeamsChanged ?? false`
- `AcceptArmutHandlerTests.cs:91,110` — update assertions to `state.Armut?.RichPlayer` / `state.Armut?.TransferCount`
- Run `dotnet test` after step 4 to confirm green before proceeding

## Migration Steps

1. **Create `ArmutState.cs`** in `Doko.Domain/GameFlow/`:
   ```csharp
   namespace Doko.Domain.GameFlow;
   public sealed record ArmutState(
       PlayerSeat Player,
       PlayerSeat? RichPlayer,
       int TransferCount,
       bool? ReturnedTrump);
   ```

2. **Create `GenscherState.cs`** in `Doko.Domain/GameFlow/`:
   ```csharp
   namespace Doko.Domain.GameFlow;
   public sealed record GenscherState(
       bool TeamsChanged,
       (PlayerSeat First, PlayerSeat Second)? PreRePlayers,
       IReadOnlyList<Announcement>? SavedAnnouncements);
   ```

3. **Update `GameState.cs`**:
   - Remove `ArmutPlayer`, `ArmutRichPlayer`, `ArmutTransferCount`, `ArmutReturnedTrump`, `GenscherTeamsChanged`, `PreGenscherRePlayers`, `SavedGenscherAnnouncements`
   - Add `public ArmutState? Armut { get; private set; }` and `public GenscherState? Genscher { get; private set; }`
   - Update `Apply()` arms:
     - `SetArmutPlayerModification` → `Armut = new ArmutState(m.ArmutPlayer, null, 0, null)`
     - `SetArmutRichPlayerModification` → `Armut = Armut! with { RichPlayer = m.RichPlayer }`
     - `ArmutGiveTrumpsModification` → compute `trumps` as before; `ArmutTransferCount` becomes `Armut = Armut! with { TransferCount = trumps.Count }`; update hands as before
     - `SetArmutReturnedTrumpModification` → `Armut = Armut! with { ReturnedTrump = m.IncludedTrump }`
     - `SetGenscherPartnerModification` (the large block) → replace field mutations with:
       - Initialize: `Genscher = new GenscherState(true, (rePlayers[0], rePlayers[1]), Announcements)`
       - Restoration: `Genscher = null` (or a zeroed record depending on downstream)
       - `GenscherTeamsChanged = true` → implicit via `Genscher is not null`; but since callers read `.TeamsChanged`, keep it explicit in the record field

4. **Update all read call sites** (Application handlers, Domain rules, Api mapper) — see Files To Touch

5. **Run `dotnet build`** — compilation errors pinpoint any missed call site; fix until clean

6. **Run `dotnet test`** — confirm all tests pass

## Acceptance Criteria

- [ ] `GameState` has no individual `ArmutPlayer`, `ArmutRichPlayer`, `ArmutTransferCount`, `ArmutReturnedTrump`, `GenscherTeamsChanged`, `PreGenscherRePlayers`, `SavedGenscherAnnouncements` properties
- [ ] `GameState.Armut is null` iff no Armut game mode is active; `GameState.Genscher is null` iff no team-changing Genscher has fired
- [ ] `dotnet build` produces zero warnings or errors
- [ ] `dotnet test` passes with no regressions

## Status
done
