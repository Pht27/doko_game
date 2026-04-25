from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
from typing import Any


@dataclass
class Tool:
    name: str
    description: str
    input_schema: dict

    def to_api_dict(self) -> dict:
        return {
            "name": self.name,
            "description": self.description,
            "input_schema": self.input_schema,
        }

    def execute(self, project_root: Path, **kwargs: Any) -> str:
        raise NotImplementedError

    def _safe_path(self, project_root: Path, rel_path: str) -> Path:
        """Resolve a path and ensure it stays within project_root."""
        p = (project_root / rel_path).resolve()
        if not str(p).startswith(str(project_root.resolve())):
            raise PermissionError(f"Path escapes project root: {rel_path}")
        return p
