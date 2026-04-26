from __future__ import annotations

import json
from dataclasses import asdict, dataclass, field
from datetime import datetime
from pathlib import Path

from todo_orchestrator.logger import get_logger

log = get_logger(__name__)


@dataclass
class WorkflowState:
    todo_path: str
    current_phase: str
    completed_phases: list[str] = field(default_factory=list)
    started_at: str = field(default_factory=lambda: datetime.now().isoformat())
    updated_at: str = field(default_factory=lambda: datetime.now().isoformat())

    def mark_complete(self, phase: str) -> None:
        if phase not in self.completed_phases:
            self.completed_phases.append(phase)
        self.updated_at = datetime.now().isoformat()

    def save(self, state_path: Path) -> None:
        data = asdict(self)
        data["updated_at"] = datetime.now().isoformat()
        state_path.write_text(json.dumps(data, indent=2))
        log.debug(f"State saved to {state_path}")

    @classmethod
    def load(cls, state_path: Path) -> "WorkflowState":
        data = json.loads(state_path.read_text())
        return cls(**data)

    @classmethod
    def state_path_for(cls, todo_path: Path) -> Path:
        return todo_path.parent / f".{todo_path.stem}_state.json"

    @classmethod
    def exists(cls, todo_path: Path) -> bool:
        return cls.state_path_for(todo_path).exists()
