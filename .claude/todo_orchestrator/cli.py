import argparse
import sys
from pathlib import Path

from todo_orchestrator.config import Config
from todo_orchestrator.logger import get_logger
from todo_orchestrator.workflow import Workflow, Phase

log = get_logger(__name__)


def parse_model_overrides(overrides: list[str]) -> dict[str, str]:
    """Parse 'research=claude-haiku-4-5' style overrides."""
    result = {}
    for item in overrides or []:
        if "=" not in item:
            log.error(f"Invalid model override (expected phase=model): {item}")
            sys.exit(1)
        phase, model = item.split("=", 1)
        result[phase.strip()] = model.strip()
    return result


def main() -> None:
    parser = argparse.ArgumentParser(
        prog="python -m todo_orchestrator",
        description="Orchestrate a structured todo workflow with specialized AI agents.",
    )
    parser.add_argument("todo_path", help="Path to the todo markdown file")
    parser.add_argument(
        "--resume",
        action="store_true",
        help="Resume from the last saved state",
    )
    parser.add_argument(
        "--step",
        choices=[p.value for p in Phase if p != Phase.DONE],
        help="Jump directly to a specific phase",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Print what would happen without calling the API or modifying files",
    )
    parser.add_argument(
        "--model",
        dest="model_overrides",
        action="append",
        metavar="PHASE=MODEL",
        help="Override model for a specific phase (e.g. research=claude-haiku-4-5-20251001)",
    )
    parser.add_argument(
        "--config",
        default=".claude/todo_orchestrator_config.yaml",
        help="Path to config file (default: .claude/todo_orchestrator_config.yaml)",
    )

    args = parser.parse_args()

    todo_path = Path(args.todo_path)
    if not todo_path.exists():
        log.error(f"Todo file not found: {todo_path}")
        sys.exit(1)

    config = Config.load(Path(args.config))
    model_overrides = parse_model_overrides(args.model_overrides)
    for phase, model in model_overrides.items():
        config.models[phase] = model

    workflow = Workflow(
        todo_path=todo_path,
        config=config,
        dry_run=args.dry_run,
    )

    start_phase = Phase(args.step) if args.step else None
    workflow.run(resume=args.resume, start_phase=start_phase)
