from __future__ import annotations

import sys
from enum import Enum
from pathlib import Path

from todo_orchestrator.agent_runner import run_agent
from todo_orchestrator.approval import ask_approval
from todo_orchestrator.config import Config
from todo_orchestrator.logger import get_logger, print_header, print_success, print_warning
from todo_orchestrator.prompt_builder import build_system_prompt, build_user_prompt
from todo_orchestrator.release_notes import (
    bump_version,
    format_version,
    parse_current_version,
    prepend_release_entry,
)
from todo_orchestrator.state import WorkflowState
from todo_orchestrator.todo_file import TodoFile
from todo_orchestrator.tools.registry import build_tool_registry

log = get_logger(__name__)


class Phase(str, Enum):
    RESEARCH = "research"
    PLAN = "plan"
    IMPLEMENT = "implement"
    REVIEW = "review"
    RELEASE_NOTES = "release_notes"
    FINISH = "finish"
    DONE = "done"


_PHASE_ORDER = [
    Phase.RESEARCH,
    Phase.PLAN,
    Phase.IMPLEMENT,
    Phase.REVIEW,
    Phase.RELEASE_NOTES,
    Phase.FINISH,
    Phase.DONE,
]

_STATUS_MAP = {
    Phase.RESEARCH: "Research",
    Phase.PLAN: "Plan",
    Phase.IMPLEMENT: "Implement",
    Phase.REVIEW: "Review",
    Phase.RELEASE_NOTES: "Release Notes",
    Phase.FINISH: "Done",
}


class Workflow:
    def __init__(self, todo_path: Path, config: Config, dry_run: bool = False) -> None:
        self.todo_path = todo_path.resolve()
        self.project_root = self._find_project_root(todo_path)
        self.config = config
        self.dry_run = dry_run
        self.todo = TodoFile(self.todo_path)
        self.state_path = WorkflowState.state_path_for(self.todo_path)

    def run(self, resume: bool = False, start_phase: Phase | None = None) -> None:
        state = self._load_or_create_state(resume)

        first_phase = start_phase or Phase(state.current_phase)
        start_idx = _PHASE_ORDER.index(first_phase)

        for phase in _PHASE_ORDER[start_idx:]:
            if phase == Phase.DONE:
                print_success("Workflow complete!")
                self._cleanup_state()
                return

            if phase.value in state.completed_phases and not start_phase:
                log.info(f"Skipping completed phase: {phase.value}")
                continue

            print_header(f"Phase: {phase.value.upper()}")
            self._run_phase(phase, state)
            state.mark_complete(phase.value)
            state.current_phase = self._next_phase(phase).value
            state.save(self.state_path)

    def _run_phase(self, phase: Phase, state: WorkflowState) -> None:
        self.todo.reload()

        if phase == Phase.RESEARCH:
            self._run_agent_phase(phase)
            self.todo.check_status_item("Research")

        elif phase == Phase.PLAN:
            self._run_agent_phase(phase)
            self.todo.check_status_item("Plan")
            plan = self.todo.get_section("Plan")
            approved = self._gate(
                "Plan Review",
                context=f"\n{plan[:1000]}\n" if plan else "(no plan written)",
            )
            if not approved:
                log.info("Plan rejected — rerunning planning phase.")
                self._run_phase(phase, state)

        elif phase == Phase.IMPLEMENT:
            self._run_agent_phase(phase)
            self.todo.check_status_item("Implement")

        elif phase == Phase.REVIEW:
            self._run_agent_phase(phase)
            self.todo.check_status_item("Review")
            review = self.todo.get_section("Review")
            approved = self._gate(
                "Implementation Review",
                context=f"\n{review[:1000]}\n" if review else "(no review written)",
            )
            if not approved:
                log.info("Implementation rejected — rerunning implement phase.")
                self._run_phase(Phase.IMPLEMENT, state)
                self._run_phase(Phase.REVIEW, state)

        elif phase == Phase.RELEASE_NOTES:
            self._run_release_notes_phase()
            self.todo.check_status_item("Release Notes")

        elif phase == Phase.FINISH:
            self._run_finish_phase()
            self.todo.check_status_item("Done")

        print_success(f"Phase complete: {phase.value}")

    def _run_agent_phase(self, phase: Phase) -> str:
        model = self.config.models.get(phase.value, "claude-sonnet-4-6")
        log.info(f"Using model: {model}")

        tools = build_tool_registry(phase.value, self.todo)
        system = build_system_prompt(phase.value, self.config, self.project_root)
        user = build_user_prompt(phase.value, self.config, self.todo, self.project_root)

        return run_agent(
            system_prompt=system,
            user_prompt=user,
            model=model,
            tools=tools,
            project_root=self.project_root,
            api_key=self.config.api_key,
            max_tokens=self.config.max_tokens,
            dry_run=self.dry_run,
        )

    def _run_release_notes_phase(self) -> None:
        rn_path = self.project_root / self.config.release_notes_file
        current = parse_current_version(rn_path)
        log.info(f"Current version: {format_version(current)}")

        model = self.config.models.get("release_notes", "claude-haiku-4-5-20251001")
        tools = build_tool_registry("release_notes", self.todo)
        system = build_system_prompt("release_notes", self.config, self.project_root)
        user = build_user_prompt("release_notes", self.config, self.todo, self.project_root)

        # Inject version info
        user += f"\n\n## Current Version\n{format_version(current)}\n\nBump 'patch' for fixes, 'minor' for new features. NEVER bump major."

        run_agent(
            system_prompt=system,
            user_prompt=user,
            model=model,
            tools=tools,
            project_root=self.project_root,
            api_key=self.config.api_key,
            max_tokens=self.config.max_tokens,
            dry_run=self.dry_run,
        )

    def _run_finish_phase(self) -> None:
        if self.dry_run:
            log.info("[DRY RUN] Would move todo, format, and commit.")
            return

        # Move todo file to 99_finished/
        finished_dir = self.todo_path.parent / self.config.finished_subdir
        finished_dir.mkdir(parents=True, exist_ok=True)
        dest = finished_dir / self.todo_path.name

        log.info(f"Moving {self.todo_path.name} → {finished_dir.name}/")

        # The finisher agent handles git. We script the move and format here
        # then let the agent handle staging + committing.
        import shutil
        shutil.move(str(self.todo_path), str(dest))
        log.info("Todo moved.")

        # Run formatter
        import subprocess
        result = subprocess.run(
            ["dotnet", "csharpier", "format", "."],
            cwd=self.project_root,
            capture_output=True,
            text=True,
            timeout=120,
        )
        if result.returncode == 0:
            log.info("CSharpier formatting done.")
        else:
            print_warning(f"Formatter warning: {result.stderr[:200]}")

        # Use the finisher agent to commit
        tools = build_tool_registry("finish", TodoFile(dest))
        system = build_system_prompt("finish", self.config, self.project_root)
        user = (
            f"The todo file has been moved to: {dest.relative_to(self.project_root)}\n"
            "CSharpier formatting has been run.\n"
            "Now commit all changes in logical, descriptive chunks.\n"
            "Use git_status to see what changed, then git_add + git_commit.\n"
            "DO NOT merge to main."
        )
        run_agent(
            system_prompt=system,
            user_prompt=user,
            model="claude-haiku-4-5-20251001",
            tools=tools,
            project_root=self.project_root,
            api_key=self.config.api_key,
            max_tokens=4096,
            dry_run=self.dry_run,
        )

    def _gate(self, label: str, context: str = "") -> bool:
        if self.dry_run:
            log.info(f"[DRY RUN] Gate: {label} — auto-approved")
            return True
        return ask_approval(label, context)

    def _load_or_create_state(self, resume: bool) -> WorkflowState:
        if resume and WorkflowState.exists(self.todo_path):
            state = WorkflowState.load(self.state_path)
            log.info(f"Resuming from phase: {state.current_phase}")
            return state
        return WorkflowState(
            todo_path=str(self.todo_path),
            current_phase=Phase.RESEARCH.value,
        )

    def _cleanup_state(self) -> None:
        if self.state_path.exists():
            self.state_path.unlink()

    def _next_phase(self, phase: Phase) -> Phase:
        idx = _PHASE_ORDER.index(phase)
        return _PHASE_ORDER[idx + 1]

    @staticmethod
    def _find_project_root(todo_path: Path) -> Path:
        """Walk up to find the git root, or use cwd."""
        p = todo_path.resolve().parent
        while p != p.parent:
            if (p / ".git").exists():
                return p
            p = p.parent
        return Path.cwd()
