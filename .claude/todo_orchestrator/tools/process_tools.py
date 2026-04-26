from __future__ import annotations

import shutil
import subprocess
from pathlib import Path
from typing import Any

from todo_orchestrator.tools.base import Tool


class RunFormatterTool(Tool):
    def __init__(self) -> None:
        super().__init__(
            name="run_formatter",
            description="Run 'dotnet csharpier format .' to format all C# backend files.",
            input_schema={"type": "object", "properties": {}, "required": []},
        )

    def execute(self, project_root: Path, **_: Any) -> str:
        if not shutil.which("dotnet"):
            return "WARNING: dotnet not found, skipping formatter."
        result = subprocess.run(
            ["dotnet", "csharpier", "format", "."],
            cwd=project_root,
            capture_output=True,
            text=True,
            timeout=120,
        )
        output = (result.stdout + result.stderr).strip()
        return output or "Formatter completed (no output)."


class MoveFileTool(Tool):
    def __init__(self) -> None:
        super().__init__(
            name="move_file",
            description="Move a file to a new location within the project. Creates parent directories as needed.",
            input_schema={
                "type": "object",
                "properties": {
                    "from_path": {"type": "string", "description": "Source path (relative to project root)"},
                    "to_path": {"type": "string", "description": "Destination path (relative to project root)"},
                },
                "required": ["from_path", "to_path"],
            },
        )

    def execute(self, project_root: Path, from_path: str, to_path: str, **_: Any) -> str:
        src = self._safe_path(project_root, from_path)
        dst = self._safe_path(project_root, to_path)
        if not src.exists():
            return f"ERROR: Source not found: {from_path}"
        dst.parent.mkdir(parents=True, exist_ok=True)
        src.rename(dst)
        return f"Moved: {from_path} → {to_path}"
