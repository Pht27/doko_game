# Spec-Driven Development Workflow

## Overview

Every feature goes through a structured lifecycle before any code is written.

## Folder Structure per Feature

```
docs/features/<feature-name>/
  spec.md       # What the feature does — requirements, rules, edge cases
  research.md   # Findings, open questions, decisions made during exploration
  plan.md       # Implementation plan — tasks, file changes, test strategy
```

## Lifecycle

1. **Spec** — Define the feature. What behaviour is required? What are the inputs/outputs?
   Acceptance criteria must be explicit and testable.

2. **Research** — Investigate unknowns. Reference game rules, existing code, edge cases.
   Document decisions and rejected alternatives.

3. **Plan** — Break the spec into concrete implementation steps.
   List files to create/modify, classes/methods to add, tests to write.

4. **Implement** — Code from the plan. No scope creep beyond the spec.

5. **Review** — Verify implementation matches spec. All acceptance criteria pass.

## Feature Naming

Feature folders use `kebab-case`, e.g.:
- `docs/features/deck-definition/`
- `docs/features/dealing/`
- `docs/features/trick-taking/`
- `docs/features/scoring/`

## Principles

- Never write code without a spec
- Specs are source of truth, not comments in code
- Plans are disposable; specs are kept up to date
