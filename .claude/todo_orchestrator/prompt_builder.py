from __future__ import annotations

import re
from pathlib import Path

from todo_orchestrator.config import Config
from todo_orchestrator.logger import get_logger
from todo_orchestrator.todo_file import TodoFile

log = get_logger(__name__)

_FRONTMATTER_RE = re.compile(r"^---\n.*?\n---\n", re.DOTALL)


def _load_text(path: Path) -> str:
    if not path.exists():
        log.warning(f"File not found, skipping: {path}")
        return ""
    return path.read_text(encoding="utf-8").strip()


def _strip_frontmatter(text: str) -> str:
    return _FRONTMATTER_RE.sub("", text).strip()


def build_system_prompt(phase: str, config: Config, project_root: Path) -> str:
    agent_path = project_root / config.agents_dir / f"{_agent_name(phase)}.md"
    agent_text = _strip_frontmatter(_load_text(agent_path))
    return agent_text or f"You are a {phase} specialist for a software project."


def build_user_prompt(phase: str, config: Config, todo: TodoFile, project_root: Path) -> str:
    template_path = project_root / config.prompts_dir / f"{phase}.md"
    template = _load_text(template_path)
    if not template:
        log.warning(f"No prompt template found for phase '{phase}', using minimal prompt.")
        template = _minimal_prompt(phase)

    substitutions = {
        "todo_content": todo.content,
        "todo_path": str(todo.path.relative_to(project_root)),
        "research_section": todo.get_section("Research"),
        "plan_section": todo.get_section("Plan"),
        "implementation_notes_section": todo.get_section("Implementation Notes"),
        "review_section": todo.get_section("Review"),
        "release_notes_content": _load_release_notes(config, project_root),
        "guidelines_hint": _guidelines_hint(config, project_root),
    }

    for key, value in substitutions.items():
        template = template.replace(f"{{{key}}}", value)

    return template


def _load_release_notes(config: Config, project_root: Path) -> str:
    path = project_root / config.release_notes_file
    return _load_text(path)


def _guidelines_hint(config: Config, project_root: Path) -> str:
    gdir = project_root / config.guidelines_dir
    if not gdir.exists():
        return ""
    files = list(gdir.rglob("*.md"))
    if not files:
        return ""
    names = [f.relative_to(project_root) for f in files[:10]]
    return "Relevant guidelines available at: " + ", ".join(str(n) for n in names)


def _agent_name(phase: str) -> str:
    return {
        "research": "research",
        "plan": "planner",
        "implement": "implementer",
        "review": "reviewer",
        "release_notes": "release-notes",
        "finish": "finisher",
    }.get(phase, phase)


def _minimal_prompt(phase: str) -> str:
    return f"Complete the {phase} phase for this todo task.\n\n## Todo\n{{todo_content}}"
