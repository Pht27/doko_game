You are reviewing the implementation against the plan.

## Todo File: {todo_path}

## Plan

{plan_section}

---

## Implementation Notes

{implementation_notes_section}

---

## Your Task

Compare the plan against the implementation:

1. Read the changed files (use read_file and grep_codebase)
2. Check each plan step: was it implemented correctly?
3. Look for: skipped steps, bugs, regressions, security issues, convention violations

When done, call `write_todo_section` with section="Review" and your findings.

Format each finding as:
- `OK` — requirement met
- `WARNING` — minor issue or concern
- `BLOCKER` — must be fixed before continuing

If everything is fine: write "LGTM" with a one-line summary.
