# Rename UseCases to Handlers

## Goal
Rename all `UseCase` classes/interfaces/folders to `Handler` throughout the codebase. Also extract private helper methods from `ExecuteAsync` methods that are long enough to warrant it.

## Implementation Plan

### Affected files
- `src/backend/Doko.Application/Games/UseCases/*.cs` (9 files) → `Games/Handlers/`
- `src/backend/Doko.Application/ServiceCollectionExtensions.cs` — DI registrations
- `src/backend/Doko.Api/Controllers/GamesController.cs` — injects interfaces
- `src/backend/Doko.Api/Mapping/DtoMapper.cs` — has comment referencing `AcceptArmutUseCase`
- `src/tests/Doko.Application.Tests/Games/UseCases/*.cs` (8 files) → `Games/Handlers/`
- `src/tests/Doko.Api.IntegrationTests/Stubs/StubUseCases.cs` → `StubHandlers.cs`
- `src/tests/Doko.Api.IntegrationTests/ApiTestFixture.cs` — references stub classes and interfaces
- `src/tests/Doko.Api.IntegrationTests/GamesControllerTests.cs` — references `StubStartGameUseCase`

### Changes per file
1. Each handler file: rename interface `I{X}UseCase` → `I{X}Handler`, class `{X}UseCase` → `{X}Handler`, namespace `...UseCases` → `...Handlers`
2. `FinishGameUseCase` has no interface — just rename class and update comment/reference in `PlayCardUseCase`
3. `ServiceCollectionExtensions.cs` — update using + all type references
4. `GamesController.cs` — update using + all constructor param types
5. `DtoMapper.cs` — update comment referencing `AcceptArmutUseCase`
6. Test files — rename class, namespace
7. `StubUseCases.cs` → `StubHandlers.cs` — rename stub classes, update interfaces
8. `ApiTestFixture.cs` — update using, interface types, stub class names
9. `GamesControllerTests.cs` — update stub class cast reference

### Readability improvements
- `AcceptArmutUseCase` (`ExecuteAsync` ~65 lines): extract `HandleAcceptance` and `HandleDecline` helpers
- `ExchangeArmutCardsUseCase` (`ExecuteAsync` ~65 lines): extract `ValidateCards`, `ComputeNewHands`, `FindStartingPlayer` helpers
- `DeclareHealthStatusUseCase` (`ExecuteAsync` ~65 lines): extract `ResolveAllDeclared` helper
- `MakeAnnouncementUseCase` is short — no change needed
- `StartGameUseCase`, `DealCardsUseCase` — reasonably readable as-is
- `MakeReservationUseCase` — already has extensive helpers
- `PlayCardUseCase` — already has extensive helpers

## Status: implemented
