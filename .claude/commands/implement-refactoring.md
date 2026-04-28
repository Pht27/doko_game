Implement one planned refactoring item (or cluster) from `docs/todos/4_refactorings/`.

## Arguments
$ARGUMENTS = path to a refactoring markdown file (e.g. `docs/todos/4_refactorings/quick-wins/domain-rename-reservation-apply.md`).
If no argument is given, use the file currently open in the IDE.

## Steps

### 0. Pre-flight checks
Read the file at `$ARGUMENTS`. Verify:

- `> **EXCLUDED**` — if present, stop. Tell the user this item is excluded.
- `## Status` — if missing or not `planned`, stop. Tell the user to run `/plan-refactoring $ARGUMENTS` first.
- `> **Depends on:**` — for each dependency, check it is `done` (has `## Status\ndone` or is in a `99_finished/` folder). If any dependency is not done, stop. Tell the user which one is missing and suggest implementing it first.

### 1. Identify the cluster
If the file has a `> **Cluster:**` note, collect all cluster partner files. All cluster files must pass the pre-flight check. Announce: "Implementing cluster: [list of files]."

### 2. Branch setup
- Check current branch: `git branch --show-current`
- If on `develop`: create a new branch named `refactor/<short-slug>` where slug derives from the file name (e.g. `domain-rename-reservation-apply`).
- If already on a feature/refactor branch: continue on it.
- Never work directly on `develop` or `main`.

### 3. Read the full plan
Re-read all cluster files, especially:
- `## Scope` — stay inside it
- `## Files To Touch` — start here
- `## Migration Steps` — follow the order exactly
- `## Test Plan` — note which tests to pin first

### 4. Pin existing behaviour (if Test Plan says so)
Before touching any source file, write or run the tests listed in `## Test Plan` as "write first". If any existing test already covers the area, run it now to confirm it's green:
```sh
dotnet test --filter "<relevant filter>"
```
Document the baseline result in `## Implementation Notes` of the cluster file.

### 5. Implement
Follow `## Migration Steps` in order. For each step:
- Make the changes.
- Update the step's checkbox in the cluster file to `[x]` when done.
- Do not deviate from the plan. If a blocker arises, document it under `## Blockers` and stop.

Stay strictly within `## Scope`. No opportunistic refactors.

### 6. Adapt / write tests
Apply the `## Test Plan`:
- Adapt existing tests that break due to interface changes.
- Write new tests called for by the plan.
- Run:
```sh
dotnet test
```
All tests must pass before continuing. If tests fail, fix them (within scope) or document under `## Blockers`.

### 7. Format
```sh
dotnet csharpier format .
```

### 8. Commit
Commit in logical chunks (one commit per migration step, or per cluster file if they interleave). Commit messages should describe *what changed and why*, not the refactoring item name.

### 9. Mark as done
In **each cluster file**, update the status section:
```
## Status
done
```

### 10. Confirm and merge
Ask the user: "Implementation complete. Shall I merge `refactor/<slug>` into `develop`?"

If confirmed:
```sh
git checkout develop
git merge --squash refactor/<slug>
git commit -m "refactor: <one-line summary>"
git branch -D refactor/<slug>
```
