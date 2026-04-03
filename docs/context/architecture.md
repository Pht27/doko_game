# Architecture

## Overview

The project follows Clean Architecture principles with a clear separation of concerns.

## Layer Structure

```
src/
  Doko.Domain/          # Core game entities, value objects, domain rules — no dependencies
  Doko.Application/     # Use cases, game logic orchestration — depends only on Domain
  Doko.Api/             # ASP.NET Core Web API — depends on Application
tests/
  Doko.Domain.Tests/
  Doko.Application.Tests/
```

## Key Design Decisions

- **Domain layer is pure**: no framework dependencies, no I/O, fully unit-testable
- **Game logic lives in Domain/Application**: the API is a thin shell
- **Spec-driven development**: every feature starts with a spec in `docs/features/`

## Naming Conventions

- Projects prefixed with `Doko.`
- Feature folders use `kebab-case`
- C# files use `PascalCase`
