# Doppelkopf

A web-based Doppelkopf card game built with .NET.

## Development Approach

This project uses **spec-driven development**. See [docs/context/spec-driven-workflow.md](docs/context/spec-driven-workflow.md).

## Documentation

- [Tech Stack](docs/context/tech-stack.md)
- [Architecture](docs/context/architecture.md)
- [Game Rules](docs/context/game-rules.md)
- [Spec-Driven Workflow](docs/context/spec-driven-workflow.md)

## Getting Started

```bash
dotnet restore
dotnet build
dotnet test
```

## CI/CD

GitHub Actions runs on every push and pull request to `main` and `develop`.
