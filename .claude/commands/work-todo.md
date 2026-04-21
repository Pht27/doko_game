Research, implement, and close out a todo item, then merge to main.

## Arguments
$ARGUMENTS should be the path to the todo markdown file (e.g. `docs/todos/todo_fix_csharpier_problems.md`).
If no argument is given, use the file currently open in the IDE.

## Steps

1. **Read the todo file** at `$ARGUMENTS` to understand what needs to be done.

2. **Research** — explore the codebase to understand the current state. Find all relevant files, patterns, and existing conventions related to the task.

3. **Plan** — write a concise implementation plan directly into the todo markdown file. Include:
   - What needs to change and why
   - Which files are affected
   - Any non-obvious decisions or trade-offs
   - When doing frontend, always think in a mobile friendly way! This is mainly going to be used as an app
   - If there is a contradiction of what the input says to what the rules say (or the rules do not have a paragraph about that situation / rule), note that and update the rules accordingly.

4. **Ask for confirmation** - Let the user decide if they like the plan. If not, go back to the drawing board.

5. **Implement** the changes described in the plan.

6. **Ask for confirmation** - Let the user decide if they like the changes or had something else in mind. If declined, go back to step 5 or even 3. DO NOT ADVANCE FURTHER WITHOUT CONFIRMATION.

7. **Mark as finished** — move the todo file into a `finished/` subfolder next to its current location (e.g. `docs/todos/99_finished/todo_fix_csharpier_problems.md`). Create the subfolder if it does not exist.

8. **Commit** the changes along the way with fitting commit messages that summarizes what was done in sensible chunks. DO NOT MERGE TO MAIN.
