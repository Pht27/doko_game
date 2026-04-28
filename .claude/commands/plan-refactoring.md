Plan one refactoring item (or cluster) from `docs/todos/4_refactorings/`.

## Arguments
$ARGUMENTS = path to a refactoring markdown file (e.g. `docs/todos/4_refactorings/quick-wins/domain-rename-reservation-apply.md`).
If no argument is given, use the file currently open in the IDE.

## Steps

### 1. Read the refactoring file
Read the file at `$ARGUMENTS`. Check for:
- `> **EXCLUDED**` — if present, stop and tell the user this item was excluded and why.
- `> **Depends on:**` lines — collect every dependency listed.
- `> **Cluster:**` lines — collect cluster partners.
- A `## Plan` section that is already filled in — if present, tell the user the item is already planned and ask if they want to re-plan it.

### 2. Check dependencies
For each dependency listed, find its file in `docs/todos/4_refactorings/` and check whether it contains a `## Status` section marked `done` or has been moved to a `99_finished/` subfolder.

If any dependency is **not yet implemented**:
- Stop.
- Tell the user which dependency is missing.
- Show the path to the dependency file.
- Suggest running `/plan-refactoring <dependency-path>` and then `/implement-refactoring <dependency-path>` first.
- Do not proceed further.

### 3. Identify the cluster
If the file has a `> **Cluster:**` note, read those partner files too. All cluster members must be planned and implemented together. Announce to the user: "This item is part of a cluster — I will plan [list] as one unit."

### 4. Research the codebase
For each file in the cluster:
- Read the description and the `## Files To Touch` section (may be empty — fill it in as part of planning).
- Locate the relevant source files referenced in `> See:` (the analysis MD files).
- Explore the actual code to understand current state, conventions, and what exactly needs to change.
- Check `.claude/guidelines` for applicable coding standards.

### 5. Write the plan
Fill in the empty sections of **each cluster file**:

```
## Scope
- What is in scope (bullet list)
- What is explicitly out of scope

## Files To Touch
- `path/to/File.cs` — what changes and why

## Test Plan
- Which existing tests cover this area (grep for them)
- What new tests to add
- Any snapshot/integration tests to write first to pin current behaviour

## Migration Steps
1. Numbered steps in the order they must happen
2. If multi-PR, indicate PR boundaries

## Acceptance Criteria
- [ ] Concrete, verifiable checklist items
```

Keep each section concise — an implementer must be able to execute without ambiguity.

### 6. Add status section
Append to each cluster file:

```
## Status
planned
```

### 7. Confirm with user
Summarise the plan and ask the user to confirm before finishing. If they request changes, update the plan and ask again.
