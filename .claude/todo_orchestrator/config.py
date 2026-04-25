from __future__ import annotations

import os
from dataclasses import dataclass, field
from pathlib import Path

import yaml

try:
    from dotenv import load_dotenv
    load_dotenv()
except ImportError:
    pass

_DEFAULTS = {
    "research": "claude-haiku-4-5-20251001",
    "plan": "claude-sonnet-4-6",
    "implement": "claude-sonnet-4-6",
    "review": "claude-haiku-4-5-20251001",
    "release_notes": "claude-haiku-4-5-20251001",
}

_ENV_MAP = {
    "research": "DEFAULT_RESEARCH_MODEL",
    "plan": "DEFAULT_PLAN_MODEL",
    "implement": "DEFAULT_IMPLEMENT_MODEL",
    "review": "DEFAULT_REVIEW_MODEL",
    "release_notes": "DEFAULT_RELEASE_MODEL",
}


@dataclass
class Config:
    api_key: str
    models: dict[str, str] = field(default_factory=dict)
    agents_dir: Path = Path(".claude/agents")
    prompts_dir: Path = Path(".claude/prompts")
    guidelines_dir: Path = Path(".claude/guidelines")
    release_notes_file: Path = Path("RELEASENOTES.md")
    finished_subdir: str = "99_finished"
    max_tokens: int = 8096

    @classmethod
    def load(cls, config_path: Path) -> "Config":
        raw: dict = {}
        if config_path.exists():
            with open(config_path) as f:
                raw = yaml.safe_load(f) or {}

        api_key = os.environ.get("ANTHROPIC_API_KEY", "")
        if not api_key:
            raise ValueError("ANTHROPIC_API_KEY not set in environment or .env file")

        models: dict[str, str] = {}
        file_models = raw.get("models", {})
        for phase, default in _DEFAULTS.items():
            env_val = os.environ.get(_ENV_MAP[phase], "")
            models[phase] = env_val or file_models.get(phase, default)

        paths = raw.get("paths", {})
        return cls(
            api_key=api_key,
            models=models,
            agents_dir=Path(paths.get("agents", ".claude/agents")),
            prompts_dir=Path(paths.get("prompts", ".claude/prompts")),
            guidelines_dir=Path(paths.get("guidelines", ".claude/guidelines")),
            release_notes_file=Path(paths.get("release_notes", "RELEASENOTES.md")),
            finished_subdir=paths.get("finished_dir", "99_finished"),
            max_tokens=raw.get("max_tokens", 8096),
        )
