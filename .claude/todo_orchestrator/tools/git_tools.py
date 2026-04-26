from __future__ import annotations

import subprocess
from pathlib import Path
from typing import Any

from todo_orchestrator.tools.base import Tool

_ALLOWED_GIT_SUBCOMMANDS = {"add", "commit", "status"}


def _run_git(project_root: Path, args: list[str]) -> str:
    result = subprocess.run(
        ["git"] + args,
        cwd=project_root,
        capture_output=True,
        text=True,
        timeout=60,
    )
    output = (result.stdout + result.stderr).strip()
    return output or "(no output)"


class GitStatusTool(Tool):
    def __init__(self) -> None:
        super().__init__(
            name="git_status",
            description="Show the current git status of the repository.",
            input_schema={"type": "object", "properties": {}, "required": []},
        )

    def execute(self, project_root: Path, **_: Any) -> str:
        return _run_git(project_root, ["status"])


class GitAddTool(Tool):
    def __init__(self) -> None:
        super().__init__(
            name="git_add",
            description="Stage files for commit.",
            input_schema={
                "type": "object",
                "properties": {
                    "paths": {
                        "type": "array",
                        "items": {"type": "string"},
                        "description": "File paths to stage (relative to project root)",
                    }
                },
                "required": ["paths"],
            },
        )

    def execute(self, project_root: Path, paths: list[str], **_: Any) -> str:
        return _run_git(project_root, ["add"] + paths)


class GitCommitTool(Tool):
    def __init__(self) -> None:
        super().__init__(
            name="git_commit",
            description="Create a git commit with the given message.",
            input_schema={
                "type": "object",
                "properties": {"message": {"type": "string", "description": "Commit message"}},
                "required": ["message"],
            },
        )

    def execute(self, project_root: Path, message: str, **_: Any) -> str:
        return _run_git(project_root, ["commit", "-m", message])
