Run the full todo workflow for a given todo file.

## Arguments
$ARGUMENTS = path to todo file (fallback: file currently open in the IDE)

---

## Steps

1. Run `/todo-research $ARGUMENTS`
2. Run `/todo-plan $ARGUMENTS`

3. Ask the user:
   "Do you approve the plan?"
   - If NO → rerun `/todo-plan $ARGUMENTS`
   - If YES → continue

4. Run `/todo-implement $ARGUMENTS`
5. Run `/todo-review $ARGUMENTS`

6. Ask the user:
   "Do you approve the implementation?"
   - If NO → go back to `/todo-implement $ARGUMENTS`
   - If YES → continue

7. Run `/todo-release-notes $ARGUMENTS`
8. Run `/todo-finish $ARGUMENTS`
