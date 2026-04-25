from __future__ import annotations

from pathlib import Path
from typing import Any

from todo_orchestrator.tools.base import Tool

try:
    from rich.console import Console
    from rich.prompt import Prompt
    _console = Console()
    def _ask(question: str) -> str:
        _console.print(f"\n[bold cyan]Planner asks:[/bold cyan] {question}")
        return Prompt.ask("  Your answer")
except ImportError:
    def _ask(question: str) -> str:
        print(f"\nPlanner asks: {question}")
        return input("  Your answer: ")


class WriteTodoSectionTool(Tool):
    def __init__(self, todo_file: Any) -> None:
        self._todo = todo_file
        super().__init__(
            name="write_todo_section",
            description=(
                "Write content into a named section of the todo file. "
                "Replaces existing content in that section. "
                "Valid sections: Research, Plan, Implementation Notes, Blockers, Review, Release Notes."
            ),
            input_schema={
                "type": "object",
                "properties": {
                    "section": {"type": "string", "description": "Section name (e.g. 'Research')"},
                    "content": {"type": "string", "description": "Markdown content for the section"},
                },
                "required": ["section", "content"],
            },
        )

    def execute(self, project_root: Path, section: str, content: str, **_: Any) -> str:
        self._todo.set_section(section, content)
        return f"Wrote to section '{section}'"


class UpdateTodoStatusTool(Tool):
    def __init__(self, todo_file: Any) -> None:
        self._todo = todo_file
        super().__init__(
            name="update_todo_status",
            description="Mark a status item in the todo's ## Status section as done.",
            input_schema={
                "type": "object",
                "properties": {
                    "item": {"type": "string", "description": "Name of the status item, e.g. 'Research'"},
                },
                "required": ["item"],
            },
        )

    def execute(self, project_root: Path, item: str, **_: Any) -> str:
        self._todo.check_status_item(item)
        return f"Checked: {item}"


class AskUserTool(Tool):
    def __init__(self) -> None:
        super().__init__(
            name="ask_user",
            description=(
                "Ask the user a clarifying question before writing the plan. "
                "Use this when the todo is ambiguous, underspecified, or has multiple valid approaches "
                "that require a decision. Do NOT ask about obvious things you can determine from the codebase."
            ),
            input_schema={
                "type": "object",
                "properties": {
                    "question": {"type": "string", "description": "The clarifying question to ask the user"},
                },
                "required": ["question"],
            },
        )

    def execute(self, project_root: Path, question: str, **_: Any) -> str:
        answer = _ask(question)
        return f"User answered: {answer}"
