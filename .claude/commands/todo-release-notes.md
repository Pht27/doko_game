Load agent: .claude/agents/release-notes.md

## Arguments
$ARGUMENTS = path to todo file (fallback: file currently open in the IDE)

## Steps

1. Read the todo file at $ARGUMENTS to understand what was changed
2. Read `RELEASENOTES.md` to find the current version
3. Classify the change and determine the version bump:
   - Small fix / minor adjustment → bump patch: X.Y.Z → X.Y.(Z+1)
   - New feature (independent, visible to users) → bump minor: X.Y.Z → X.(Y+1).0
   - NEVER bump major version (X)
4. Update `RELEASENOTES.md` with the new version and a concise summary

## Output

Write a short summary of the release notes entry into the todo file under:

## Release Notes
