Load agent: .claude/agents/reviewer.md

## Arguments
$ARGUMENTS = path to todo file (fallback: file currently open in the IDE)

## Steps

1. Read the todo file at $ARGUMENTS (Plan + Implementation Notes sections)
2. Inspect the actual code changes (git diff or relevant files)
3. Compare plan against implementation

## Output

Write findings directly into the todo file under:

## Review
