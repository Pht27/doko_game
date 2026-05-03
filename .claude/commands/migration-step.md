Research, plan, implement, and close out one migration step from `docs/todos/5_migration/`.

## Arguments
$ARGUMENTS = path to a migration step file (e.g. `docs/todos/5_migration/02_doko_analog_projekt.md`).
If no argument is given, use the file currently open in the IDE.

---

## Steps

### 0. Pre-flight checks

Read the file at `$ARGUMENTS`. Then read `docs/todos/5_migration/00_gesamtplan.md`.

Verify:
- The step's status in `00_gesamtplan.md` is `offen` — if already `erledigt`, stop and tell the user.
- All dependencies listed for this step in `00_gesamtplan.md` are `erledigt`. If not, stop and name the missing dependency.

### 1. Branch setup

- Check current branch: `git branch --show-current`
- If on `develop`: create a new branch `migration/<short-slug>` where slug derives from the file name (e.g. `02-doko-analog-projekt`).
- If already on a migration/feature branch: continue on it.
- Never work directly on `develop` or `main`.

### 2. Research

Read the step file thoroughly. Then explore the codebase:
- Find all relevant files, patterns, and existing conventions for this step.
- Check `.claude/guidelines` for any fitting guidelines.
- For backend steps: look at existing project structure, how other services/controllers are organized.
- For frontend steps: look at existing pages, components, hooks, and API clients.
- For infrastructure steps (server, DB): read current config files (`appsettings.json`, `Program.cs`, `Doko.sln`).

Write a short **Research** section directly into the step file.

### 3. Plan

Write a concrete implementation plan into the step file under a `## Plan` section:
- Exact files to create or modify
- Order of changes
- Non-obvious decisions or trade-offs
- For frontend steps: think mobile-first (the app is primarily used on phones)
- Note anything that contradicts or extends the original step description

### 4. Ask for confirmation

Present the plan and ask: "Plan OK? Dann fangen wir an."
- If NO → revise the plan
- If YES → continue

### 5. Implement

Follow the plan. For each significant sub-step:
- Make the changes.
- Run tests if applicable: `dotnet test` (backend) or `npm run build` (frontend TypeScript check).
- Do not deviate from the plan. If a blocker arises, document it under `## Blockers` in the step file and stop.

Stay strictly within the scope of this step. No opportunistic changes to unrelated code.

### 6. Ask for confirmation

Show what was implemented and ask: "Implementierung OK?"
- If NO → revise (go back to step 5 or even step 3)
- If YES → continue

### 7. Format

If backend files were changed:
```sh
dotnet csharpier format .
```

If frontend files were changed: the build check from step 5 is sufficient.

### 8. Update release notes

Only if this step introduced **user-visible changes** (new pages, changed behavior, new API features):

Add to the current latest version block at the top of `RELEASENOTES.md`:
- Write a concise, user-focused summary in **German**
- Style: short sentences, user perspective ("Spieler können jetzt..." not "Endpoint X hinzugefügt")
- Use `### Neu`, `### Behoben`, or `### Verbessert` as appropriate

Skip this step for infrastructure-only changes (server setup, DB schema, project structure).

### 9. Mark step as done

1. Update the step file: add `## Status\nerledigt` at the bottom.
2. Move the step file to `docs/todos/5_migration/99_finished/` (create the folder if needed).
3. Update `docs/todos/5_migration/00_gesamtplan.md`: change the step's status from `offen` to `erledigt`.

### 10. Commit

Commit in logical chunks with descriptive messages (what changed and why). Example commit message prefixes:
- `feat:` for new features / endpoints / pages
- `chore:` for infrastructure, project setup, config
- `refactor:` for structural changes without new behavior

### 11. Merge into develop

Ask the user: "Soll ich `migration/<slug>` in `develop` mergen?"

If confirmed:
```sh
git checkout develop
git merge --squash migration/<slug>
git commit -m "migration: <one-line summary of this step>"
git push origin develop
git branch -d migration/<slug>
```
