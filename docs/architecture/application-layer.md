# Application Layer — Architecture

## Context

The Domain layer is complete. This document defines the design of `Doko.Application` — the orchestration shell between Domain and API.

---

## Goals

1. **Load and save game state** — every command is framed by `GetAsync` / `SaveAsync`
2. **Authorize the action** — validate the requesting `PlayerId` is allowed to act (turn check)
3. **Invoke domain services** — `CardPlayValidator`, `AnnouncementRules`, `IGameScorer`; never re-implement rules
4. **Drive phase transitions** — decide when to advance `GamePhase` (all reservations in → resolve → Playing; last trick → scoring → Finished)
5. **Publish domain events** — after save, fan out events via `IGameEventPublisher` abstraction (SignalR impl lives in Api)
6. **Return typed errors** — discriminated union result type; no exceptions for expected failures

---

## Key Design Decisions

### CQRS
Lightweight split only:
- **Commands** → use case objects returning `GameActionResult<T>`
- **Queries** → `IGameQueryService` returning per-player projections (`PlayerGameView`)

No separate event store, no event replay, no separate read DB. Just "load snapshot, project filtered view."

### Persistence (`IGameRepository`)
Persist full `GameState` snapshot. `GetAsync` / `SaveAsync`.

`TrumpEvaluator` and `PartyResolver` are interfaces — cannot be naively serialized. The repository reconstructs them from `ActiveReservation` (via `IReservation.Apply()`) and `ActiveSonderkarten` (via `RebuildTrumpEvaluator`) on load.

### Domain Events
`GameState` has no event collection list. Use cases construct events themselves from before/after state.

After save: call `IGameEventPublisher.PublishAsync(gameId, events)`. The Api-layer `SignalRGameEventPublisher` translates to hub messages.

### Mediator
No MediatR. Plain use case classes with constructor-injected dependencies.

### Read Model
The API must not expose full `GameState` (other players' hand cards would leak). `IGameQueryService.GetPlayerViewAsync(gameId, requestingPlayerId)` returns a `PlayerGameView` containing:
- Only the requesting player's hand
- `LegalCards` and `LegalAnnouncements` pre-computed (so UI doesn't implement rules)
- `EligibleSonderkartenPerCard` — for each card in hand, which Sonderkarten the player *could* activate by playing it (pre-computed via `SonderkarteRegistry.GetEligibleForCard`)
- Other players: seat, known party, hand card count only

### Sonderkarte Activation — Player Decision at Card Play Time
Sonderkarten are triggered by playing a specific card (`ISonderkarte.TriggeringCard`). The player declares activation **in the same action as playing the card** — no separate roundtrip.

Bundled into `PlayCardCommand`:
```csharp
public record PlayCardCommand(GameId GameId, PlayerId Player, CardId Card,
    IReadOnlyList<SonderkarteType> ActivateSonderkarten);  // player declares opt-in
```

`PlayCardUseCase` flow (after card validity check, before removing from hand):
1. Call `SonderkarteRegistry.GetEligibleForCard(card, state, rules)` → eligible set
2. For each type in `command.ActivateSonderkarten`: verify it is in eligible set → `GameError.SonderkarteNotEligible` if not
3. For each activated type: call `sonderkarte.Apply(state)` and apply the resulting `GameStateModification` (if non-null)
4. Raise `SonderkarteTriggeredEvent` for each

Omitting an eligible Sonderkarte forfeits it (opt-in). The UI uses `EligibleSonderkartenPerCard` from `PlayerGameView` to prompt the player.

### Error Handling
```csharp
public abstract record GameActionResult<T> {
    public sealed record Ok(T Value) : GameActionResult<T>;
    public sealed record Failure(GameError Error) : GameActionResult<T>;
}
public enum GameError {
    GameNotFound,
    NotYourTurn,
    InvalidPhase,
    IllegalCard,
    AnnouncementNotAllowed,
    ReservationNotEligible,
    SonderkarteNotEligible
}
```
Infrastructure failures remain exceptions, handled globally in Api.

### Phase Transitions — Domain Gap
`GameState.Apply()` currently handles only 4 modification types. Phase transitions require extending the `GameStateModification` sealed hierarchy in Domain:
- `AdvancePhaseModification(GamePhase NewPhase)`
- `SetGameModeModification(IReservation? Reservation)` — sets `ActiveReservation`, rebuilds evaluator/resolver
- `SetCurrentTurnModification(PlayerId Player)`
- `DealHandsModification(Dictionary<PlayerId, Hand> Hands)`

This keeps `GameState.Apply()` as the single mutation point.

### `GameState.Create()` — Existing Gap
Currently hardcodes `Phase = GamePhase.Playing`. Must default to `GamePhase.Dealing` for normal game start.

### Shuffling
`DealCardsUseCase` needs non-deterministic deck ordering. Extracted for testability:
```csharp
public interface IDeckShuffler { IReadOnlyList<Card> Shuffle(IReadOnlyList<Card> deck); }
```
Registered in Api; test fake uses fixed seed.

### `FinishGameUseCase`
Internal — called by `PlayCardUseCase` after the last trick completes. Not exposed as a public interface to Api.

### Reservation Phase — Turn Order
Reservations are declared asynchronously (not strict turn-based). `MakeReservationUseCase` accepts from any player who hasn't declared yet. When all four have declared: resolve by `ReservationPriority`, apply winning reservation, advance phase.

---

## Project Structure

```
src/
  Doko.Application/
    Doko.Application.csproj          (depends on Doko.Domain only)
    Abstractions/
      IGameRepository.cs
      IGameEventPublisher.cs
      IGameQueryService.cs
      IDeckShuffler.cs
    Common/
      GameActionResult.cs
      GameError.cs
      Unit.cs
    Games/
      Commands/
        StartGameCommand.cs
        DealCardsCommand.cs
        MakeReservationCommand.cs
        PlayCardCommand.cs
        MakeAnnouncementCommand.cs
      Results/
        StartGameResult.cs
        PlayCardResult.cs
        MakeReservationResult.cs
        GameFinishedResult.cs
      Queries/
        PlayerGameView.cs
        PlayerPublicState.cs
        TrickSummary.cs
      UseCases/
        StartGameUseCase.cs
        DealCardsUseCase.cs
        MakeReservationUseCase.cs
        PlayCardUseCase.cs
        MakeAnnouncementUseCase.cs
        FinishGameUseCase.cs          (internal, not exposed to Api)
      GameQueryService.cs
    ServiceCollectionExtensions.cs   (AddDokoApplication())

tests/
  Doko.Application.Tests/
    Games/UseCases/
      StartGameUseCaseTests.cs
      PlayCardUseCaseTests.cs
      ...
    Fakes/
      InMemoryGameRepository.cs
      RecordingGameEventPublisher.cs
```

---

## Public Interfaces Exposed to Api

```csharp
IStartGameUseCase          → Task<GameActionResult<StartGameResult>>
IDealCardsUseCase          → Task<GameActionResult<Unit>>
IMakeReservationUseCase    → Task<GameActionResult<MakeReservationResult>>
IPlayCardUseCase           → Task<GameActionResult<PlayCardResult>>
IMakeAnnouncementUseCase   → Task<GameActionResult<Unit>>
IGameQueryService          → Task<PlayerGameView?>
```

Api registers infrastructure implementations of: `IGameRepository`, `IGameEventPublisher`, `IDeckShuffler`.

---

## Domain Changes Needed Before Application Layer

Prerequisite fixes in `Doko.Domain`:

1. **`GameState.Create()`** — default `Phase` to `GamePhase.Dealing` (currently hardcodes `Playing`)
2. **New `GameStateModification` subtypes** — `AdvancePhaseModification`, `SetGameModeModification`, `SetCurrentTurnModification`, `DealHandsModification`
3. **`GameState.Apply()`** — handle the four new modification types above
4. **`Hand.Remove(Card)`** — needed by `PlayCardUseCase`
5. **`Deck.Standard48()` / `Deck.Standard40()`** — needed by `DealCardsUseCase`

---

## What Does NOT Belong Here

| Concern | Belongs in |
|---|---|
| HTTP routing, JSON DTOs, auth | `Doko.Api` |
| SignalR hub | `Doko.Api` |
| Repository implementation (EF Core, Redis, etc.) | `Doko.Api` / Infrastructure |
| User → PlayerId mapping | `Doko.Api` |
| Any game rule logic | `Doko.Domain` services |

---

## Verification

- `dotnet build` — no warnings, clean compile
- `dotnet test tests/Doko.Application.Tests` — use case tests with `InMemoryGameRepository` and `RecordingGameEventPublisher`
- Smoke test: `StartGame → DealCards → MakeReservation (×4) → PlayCard (×48) → verify GameFinishedResult returned`
