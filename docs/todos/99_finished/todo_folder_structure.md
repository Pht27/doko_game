I would like a clean folder structure, where all the code is in the src files. The assets like the cards have to lie in another folder. the src folder is divided into frontend and backend (all the c# stuff). Later we will add Deployment (CI/CD) and Database code. When changing this, the solution needs to be adapted.

## Implementation Plan

### Target Structure
```
/
├── Doko.sln
├── src/
│   ├── frontend/     (was frontend/)
│   ├── backend/      (was src/ — all C# projects)
│   └── tests/        (was tests/)
├── assets/           (was resources/ — card SVGs etc.)
├── docs/
├── .github/
└── .config/
```

### Files Affected
- `Doko.sln` — update all project paths
- `src/tests/*/` — update ProjectReference paths from `..\..\src\Doko.X\` to `..\..\backend\Doko.X\`
- Within-backend `.csproj` references stay the same (all siblings under `src/backend/`)
- CI workflow unchanged (still runs `dotnet restore/build/test` at root with `Doko.sln`)

### Trade-offs
- `resources/` → `assets/` rename: frontend still has its own copies in `src/frontend/public/cards/` and `src/frontend/src/assets/cards/` for Vite — not touched
- Solution virtual folders (src/tests) in `.sln` are display-only; keeping names as-is, only paths update
