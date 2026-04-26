from __future__ import annotations

import re
from datetime import date
from pathlib import Path

from todo_orchestrator.logger import get_logger

log = get_logger(__name__)

_VERSION_RE = re.compile(r"##\s+\[(\d+)\.(\d+)\.(\d+)\]")


def parse_current_version(release_notes_path: Path) -> tuple[int, int, int]:
    if not release_notes_path.exists():
        return (1, 0, 0)
    content = release_notes_path.read_text(encoding="utf-8")
    m = _VERSION_RE.search(content)
    if not m:
        return (1, 0, 0)
    return int(m.group(1)), int(m.group(2)), int(m.group(3))


def bump_version(current: tuple[int, int, int], bump_type: str) -> tuple[int, int, int]:
    major, minor, patch = current
    if bump_type == "minor":
        return (major, minor + 1, 0)
    return (major, minor, patch + 1)


def format_version(v: tuple[int, int, int]) -> str:
    return f"{v[0]}.{v[1]}.{v[2]}"


def prepend_release_entry(
    release_notes_path: Path,
    version: tuple[int, int, int],
    entry_body: str,
) -> str:
    """Prepend a new version block and return the new version string."""
    version_str = format_version(version)
    today = date.today().isoformat()
    header = f"## [{version_str}] - {today}"
    new_block = f"{header}\n\n{entry_body.strip()}\n"

    if release_notes_path.exists():
        existing = release_notes_path.read_text(encoding="utf-8")
        # Insert after the top-level # heading if present
        if existing.startswith("#"):
            first_newline = existing.index("\n")
            content = existing[: first_newline + 1] + "\n" + new_block + "\n" + existing[first_newline + 1:].lstrip("\n")
        else:
            content = new_block + "\n" + existing
    else:
        content = f"# Release Notes\n\n{new_block}"

    release_notes_path.write_text(content, encoding="utf-8")
    log.info(f"Updated {release_notes_path} → v{version_str}")
    return version_str
