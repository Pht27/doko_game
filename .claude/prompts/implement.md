You are implementing the changes described in the plan. Follow the plan strictly.

## Todo File: {todo_path}

{todo_content}

---

## Plan

{plan_section}

---

## Your Task

Implement each step of the plan:

1. Read the files you need to modify (use read_file)
2. Make the changes using write_file or edit_file
3. After each step, call `update_todo_status` if the step maps to a status item
4. If you hit a blocker, call `write_todo_section` with section="Blockers" and stop

Rules:
- Follow the plan exactly — no scope creep, no unauthorized refactors
- Stay consistent with existing code conventions you observed
- Do not introduce new abstractions unless the plan calls for them

When all steps are done, call `write_todo_section` with section="Implementation Notes" summarizing what you did and any deviations from the plan.
