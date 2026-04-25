You are creating an implementation plan for a todo task. Do NOT write any code.

## Todo File: {todo_path}

{todo_content}

---

## Research Findings

{research_section}

---

## Guidelines

{guidelines_hint}

---

## Your Task

**Step 1 — Clarify if needed**

Before writing the plan, use `ask_user` if anything is genuinely unclear:
- The todo is vague and multiple valid interpretations exist
- A key decision (approach, scope, behaviour) requires human input
- A constraint or edge case is missing that would significantly change the plan

Do NOT ask about things you can determine from the codebase or research findings.
Do NOT ask unnecessary questions — if it's obvious, proceed.

**Step 2 — Explore**

Read any files you need to verify your understanding (use read_file).

**Step 3 — Write the plan**

Create a clear, numbered implementation plan:

1. What needs to change and why
2. All affected files with their paths
3. Trade-offs, edge cases, and risks
4. For any frontend changes: mobile-first (this app is a PWA used on mobile)
5. If the task contradicts existing rules or conventions, document it under **Rule Conflicts** and propose a resolution

When ready, call `write_todo_section` with section="Plan" and your complete plan.

The plan must be detailed enough that another engineer can implement it without ambiguity.
