Research, implement, and close out a todo item, then merge to main.

## Arguments
$ARGUMENTS should be the path to the todo markdown file (e.g. `docs/todos/todo_fix_csharpier_problems.md`).
If no argument is given, use the file currently open in the IDE.

## Steps

1. **Read the todo file** at `$ARGUMENTS` to understand what needs to be done.

2. **Research** — explore the codebase to understand the current state. Find all relevant files, patterns, and existing conventions related to the task. Check in `.claude/guidelines` if you should review any fitting guidelines.

3. **Plan** — write a concise implementation plan directly into the todo markdown file. Include:
   - What needs to change and why
   - Which files are affected
   - Any non-obvious decisions or trade-offs
   - When doing frontend, always think in a mobile friendly way! This is mainly going to be used as an app
   - If there is a contradiction of what the input says to what the rules say (or the rules do not have a paragraph about that situation / rule), note that and update the rules accordingly.

4. **Ask for confirmation** - Let the user decide if they like the plan. If not, go back to the drawing board.

5. **Implement** the changes described in the plan.

6. **Ask for confirmation** - Let the user decide if they like the changes or had something else in mind. If declined, go back to step 5 or even 3. DO NOT ADVANCE FURTHER WITHOUT CONFIRMATION.

7. **Update release notes and version** — based on what was implemented:
   - Read the current version from `RELEASENOTES.md`
   - Classify the change:
     - Small fix / minor adjustment → bump patch: `X.Y.Z → X.Y.(Z+1)`
     - New independent feature visible to users → bump minor: `X.Y.Z → X.(Y+1).0`
     - Never bump the major version (X)
   - Add a new section at the top of `RELEASENOTES.md` (after the `# Release Notes` heading):
     `## [X.Y.Z] - YYYY-MM-DD`
   - Write a concise, user-focused summary in **German**
   - Style: short sentences, user perspective ("Spieler sehen jetzt..." not "Methode X hinzugefügt"), no implementation details
   - Use subsections `### Neu`, `### Behoben`, or `### Verbessert` as appropriate
   - Also update the `"version"` field in `Code/frontend/package.json` to the same new version string

8. **Mark as finished** — move the todo file into a `99_finished/` subfolder next to its current location (e.g. `docs/todos/99_finished/todo_fix_csharpier_problems.md`). Create the subfolder if it does not exist.

9. Run `dotnet csharpier format .` to format backend files if any changed.

10. **Commit** the changes along the way with fitting commit messages that summarizes what was done in sensible chunks. DO NOT MERGE TO MAIN.
