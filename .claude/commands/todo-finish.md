Load agent: .claude/agents/finisher.md

## Arguments
$ARGUMENTS = path to todo file (fallback: file currently open in the IDE)

## Steps

1. Move the todo file into the `99_finished/` subfolder next to its current location
   (e.g. `docs/todos/todo_foo.md` → `docs/todos/99_finished/todo_foo.md`)
   Create the subfolder if it does not exist.

2. Run `dotnet csharpier format .` to format any changed backend files

3. Commit all changes in logical, descriptive chunks

DO NOT merge to main.
