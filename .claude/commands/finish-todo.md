Research, implement, and close out a todo item, then merge to main.

## Arguments
$ARGUMENTS should be the path to the todo markdown file (e.g. `docs/todos/todo_fix_csharpier_problems.md`).
If no argument is given, use the file currently open in the IDE. Create a new branch for this implementation.

## Steps

1. **Read the todo file** at `$ARGUMENTS` to understand what needs to be done.

2. **Research** — explore the codebase to understand the current state. Find all relevant files, patterns, and existing conventions related to the task.

3. **Plan** — write a concise implementation plan directly into the todo markdown file. Include:
   - What needs to change and why
   - Which files are affected
   - Any non-obvious decisions or trade-offs

4. **Implement** the changes described in the plan.

5. **Mark as finished** — move the todo file into a `finished/` subfolder next to its current location (e.g. `docs/todos/finished/todo_fix_csharpier_problems.md`). Create the subfolder if it does not exist.

6. **Commit** the changes along the way with fitting commit messages that summarizes what was done in sensible chunks.
