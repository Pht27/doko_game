# Plan: API Layer

## Status

| Project | Status |
|---------|--------|
| `Doko.Infrastructure` | ✅ Done — `InMemoryGameRepository`, `RandomDeckShuffler`, `AddDokoInfrastructure()` |
| `Doko.Console` | ✅ Done — full hot-seat game loop, renderer, input reader, event publisher |
| `Doko.Api` | ⬜ Not started |

## Context

Domain and Application layers are complete with 147 passing tests. The next step is to expose the game engine over HTTP so clients can play, and to provide a local console runner for testing game logic without a server. This plan introduces three new projects: `Doko.Api` (controllers, DTOs, SignalR hub, DI wiring), `Doko.Infrastructure` (in-memory repository, event publisher, deck shuffler), and `Doko.Console` (interactive terminal game).

Choices made:
- **Auth**: JWT bearer tokens; a simple `/auth/token` endpoint issues tokens with a `player_id` claim (no full user management yet)
- **Real-time**: SignalR hub pushes domain events to clients after each command
- **Persistence**: In-memory `IGameRepository` (dictionary-backed) — sufficient for live game state; history/scoring DB deferred
- **Projects**: Separate `Doko.Api`, `Doko.Infrastructure`, and `Doko.Console`

---

## New Projects

### `src/Doko.Api`
ASP.NET Core Web API targeting .NET 10.

**Folder structure:**
```
Doko.Api/
  Program.cs
  Controllers/
    AuthController.cs
    GamesController.cs
  Hubs/
    GameHub.cs
  DTOs/
    Requests/
      StartGameRequest.cs          — { PlayerIds: int[], Rules?: RuleSetDto }
      DealCardsRequest.cs          — (empty, gameId from route)
      MakeReservationRequest.cs    — { Reservation: string?, HochzeitCondition: string?, ArmutPartner: int? }
                                        Reservation is a ReservationPriority enum name or null ("keine Vorbehalt")
                                        HochzeitCondition: "FirstTrick" | "FirstFehlTrick" | "FirstTrumpTrick"
                                        ArmutPartner: PlayerId of the rich player (required when Reservation == "Armut")
      PlayCardRequest.cs           — { CardId: int, ActivateSonderkarten: string[], GenscherPartnerId: int? }
      MakeAnnouncementRequest.cs   — { Type: string }
    Responses/
      StartGameResponse.cs         — { GameId: string }
      PlayerGameViewResponse.cs    — maps PlayerGameView
      ErrorResponse.cs             — { Error: string }
  Mapping/
    DtoMapper.cs                   — static helpers: command → DTO, domain → DTO
                                     includes BuildReservation factory (mirrors ConsoleInputReader.BuildReservation)
  Extensions/
    GameActionResultExtensions.cs  — GameActionResult<T> → IActionResult (Ok/BadRequest/NotFound)
    ServiceCollectionExtensions.cs — AddDokoApi()
```

### `src/Doko.Infrastructure`
Class library. Implements Application abstractions.

**Folder structure:**
```
Doko.Infrastructure/
  Repositories/
    InMemoryGameRepository.cs      — ConcurrentDictionary<GameId, GameState>
  Events/
    SignalRGameEventPublisher.cs   — IHubContext<GameHub> → sends events to game group
  Shuffler/
    RandomDeckShuffler.cs          — Fisher-Yates shuffle
  ServiceCollectionExtensions.cs  — AddDokoInfrastructure()
```

### `src/Doko.Console`
.NET 10 console app. A presentation layer sitting parallel to `Doko.Api` — same Application use cases, different renderer. Reuses `Doko.Infrastructure` for `IGameRepository`, `IDeckShuffler`. Runs all 4 players in one process (hot-seat), prompting each player in turn.

**Folder structure:**
```
Doko.Console/
  Program.cs                       — top-level: DI setup, game loop entry
  ConsoleGame.cs                   — orchestrates the full game loop
  Rendering/
    GameRenderer.cs                — prints PlayerGameView to terminal (hand, trick, scores)
  Input/
    ConsoleInputReader.cs          — prompts player to pick card, reservation, announcement
  Events/
    ConsoleGameEventPublisher.cs   — IGameEventPublisher: prints events as they happen
```

**Game loop (ConsoleGame.cs):**
1. `StartGameUseCase` → creates game
2. `DealCardsUseCase` → deals cards
3. Reservation round: prompt each player, call `MakeReservationUseCase`
4. Playing round: for current player, render their view, prompt card + optional sonderkarte/announcement, call `PlayCardUseCase` / `MakeAnnouncementUseCase`
5. Repeat until `PlayCardResult.GameFinished == true`
6. Print `GameResult`

**DI in Program.cs:**
```csharp
services
    .AddDokoApplication()
    .AddDokoInfrastructure()          // InMemoryGameRepository, RandomDeckShuffler
    .AddSingleton<IGameEventPublisher, ConsoleGameEventPublisher>()
    .AddSingleton<ConsoleGame>();
```

---

## Endpoints

| Method | Route | Use Case / Service | Auth |
|--------|-------|--------------------|------|
| POST | `/auth/token` | — | None |
| POST | `/games` | `IStartGameUseCase` | Required |
| POST | `/games/{id}/deal` | `IDealCardsUseCase` | Required |
| POST | `/games/{id}/reservations` | `IMakeReservationUseCase` | Required |
| POST | `/games/{id}/cards` | `IPlayCardUseCase` | Required |
| POST | `/games/{id}/announcements` | `IMakeAnnouncementUseCase` | Required |
| GET | `/games/{id}` | `IGameQueryService` | Required |

### Auth endpoint
`POST /auth/token` body: `{ "playerId": 0 }` → returns `{ "token": "..." }`  
JWT contains claim `player_id = 0..3`. No password/user management for now.

### PlayerId extraction
All controllers read `player_id` from JWT claims → `new PlayerId((byte)claim)`.

---

## SignalR

**Hub:** `GameHub` at `/hubs/game`  
- Clients join a group per game: `await Groups.AddToGroupAsync(connectionId, gameId)`
- `SignalRGameEventPublisher` holds `IHubContext<GameHub>` and sends typed events to the group

**Events pushed to clients:**
- `CardPlayed` — `{ Player, Card }`
- `TrickCompleted` — `{ Winner, Points }`
- `AnnouncementMade` — `{ Player, Type }`
- `ReservationMade` — `{ Player, Reservation? }`
- `GameFinished` — `{ Result: GameResultDto }`
- `SonderkarteTriggered` — `{ Type, Player }`

---

## Key Implementation Details

### `GameActionResult<T>` → HTTP
```csharp
// GameActionResultExtensions.cs
static IActionResult ToActionResult<T>(this GameActionResult<T> result, Func<T, IActionResult> onOk)
    => result switch {
        GameActionResult<T>.Ok ok => onOk(ok.Value),
        GameActionResult<T>.Failure f => f.Error switch {
            GameError.GameNotFound     => new NotFoundResult(),
            GameError.NotYourTurn      => new BadRequestObjectResult(new ErrorResponse("not_your_turn")),
            GameError.InvalidPhase     => new BadRequestObjectResult(new ErrorResponse("invalid_phase")),
            GameError.IllegalCard      => new BadRequestObjectResult(new ErrorResponse("illegal_card")),
            // ... remaining errors
        }
    };
```

### `InMemoryGameRepository`
```csharp
// ConcurrentDictionary for thread safety across async handlers
private readonly ConcurrentDictionary<GameId, GameState> _store = new();

Task<GameState?> GetAsync(GameId id, ...) => Task.FromResult(_store.GetValueOrDefault(id));
Task SaveAsync(GameState state, ...) { _store[state.Id] = state; return Task.CompletedTask; }
```

### `RandomDeckShuffler`
Fisher-Yates using `Random.Shared`.

### DI wiring in `Program.cs`
```csharp
builder.Services
    .AddDokoApplication()
    .AddDokoInfrastructure()
    .AddDokoApi()
    .AddSignalR()
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(...);
```

---

## Solution File

Add all three new projects to `Doko.sln`. Also add a `tests/Doko.Api.Tests` project.

---

## Tests

New project: `tests/Doko.Api.Tests` (xUnit + `Microsoft.AspNetCore.Mvc.Testing`).

**What to test:**

### `GameActionResultExtensions` (unit tests)
- Each `GameError` maps to the correct HTTP status and error string
- `Ok` result calls the `onOk` delegate and returns its result

### `DtoMapper.BuildReservation` (unit tests)
- Each `ReservationPriority` maps to the correct `IReservation` concrete type
- `HochzeitCondition` string maps to the correct `HochzeitCondition` enum value
- `ArmutPartner` is wired into `ArmutReservation.RichPlayer`
- Unknown/null reservation string returns null (keine Vorbehalt)

### `AuthController` (integration tests via `WebApplicationFactory`)
- `POST /auth/token { "playerId": 0 }` returns 200 with a valid JWT containing `player_id = 0`
- `playerId` outside 0–3 returns 400

### `GamesController` (integration tests via `WebApplicationFactory`)
- Unauthenticated requests return 401
- `POST /games` with 4 player IDs returns 200 with a `GameId`
- `GET /games/{id}` with wrong player's token returns the correct player view (not another's)
- `POST /games/{id}/deal` after game start returns 200
- Use case failures map correctly (e.g. `NotYourTurn` → 400 `not_your_turn`)

**Test project DI:** Register mock/stub use cases (`Substitute` or hand-rolled stubs) via `WebApplicationFactory.WithWebHostBuilder`. Use `InMemoryGameRepository` directly for integration tests that need real game state.

---

## Critical Files

- [src/Doko.Application/ServiceCollectionExtensions.cs](../../src/Doko.Application/ServiceCollectionExtensions.cs)
- [src/Doko.Application/Abstractions/IGameRepository.cs](../../src/Doko.Application/Abstractions/IGameRepository.cs)
- [src/Doko.Application/Abstractions/IGameEventPublisher.cs](../../src/Doko.Application/Abstractions/IGameEventPublisher.cs)
- [src/Doko.Application/Abstractions/IDeckShuffler.cs](../../src/Doko.Application/Abstractions/IDeckShuffler.cs)
- [src/Doko.Application/Games/UseCases/](../../src/Doko.Application/Games/UseCases/) — all use case interfaces
- [src/Doko.Application/Games/Results/GameActionResult.cs](../../src/Doko.Application/Games/Results/GameActionResult.cs)
- [src/Doko.Application/Games/Queries/IGameQueryService.cs](../../src/Doko.Application/Games/Queries/IGameQueryService.cs)
- [src/Doko.Domain/GameFlow/GameState.cs](../../src/Doko.Domain/GameFlow/GameState.cs) — for serialization awareness

---

## Verification

**Console:**
1. `dotnet run --project src/Doko.Console` — starts a hot-seat game in the terminal
2. Each player's hand and the current trick are displayed on their turn
3. Full game plays through to a printed `GameResult`

**API:**
1. `dotnet build Doko.sln` — no errors
2. `dotnet run --project src/Doko.Api` starts on `https://localhost:5001`
3. `POST /auth/token` with `{ "playerId": 0 }` returns a JWT
4. `POST /games` with 4 player IDs returns a `GameId`
5. `POST /games/{id}/deal` transitions game to Reservations phase
6. `GET /games/{id}` (with player 0's token) returns a `PlayerGameView` with 12 cards
7. SignalR: client connects to `/hubs/game`, joins game group, receives `CardPlayed` after `POST /games/{id}/cards`
8. Full game flow: deal → reservations → play 48 cards → `GameFinished` event received
