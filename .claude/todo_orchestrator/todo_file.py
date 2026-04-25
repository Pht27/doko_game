from __future__ import annotations

import re
from pathlib import Path

from todo_orchestrator.logger import get_logger

log = get_logger(__name__)

KNOWN_SECTIONS = [
    "Status",
    "Research",
    "Plan",
    "Implementation Notes",
    "Blockers",
    "Review",
    "Release Notes",
]

_SECTION_RE = re.compile(r"^## (.+)$", re.MULTILINE)
_STATUS_ITEM_RE = re.compile(r"^- \[( |x|X)\] (.+)$", re.MULTILINE)


class TodoFile:
    def __init__(self, path: Path) -> None:
        self.path = path
        self._content = path.read_text(encoding="utf-8")

    def reload(self) -> None:
        self._content = self.path.read_text(encoding="utf-8")

    @property
    def content(self) -> str:
        return self._content

    def get_section(self, name: str) -> str:
        """Return the content of a named ## section (empty string if missing)."""
        matches = list(_SECTION_RE.finditer(self._content))
        for i, m in enumerate(matches):
            if m.group(1).strip().lower() == name.lower():
                start = m.end()
                end = matches[i + 1].start() if i + 1 < len(matches) else len(self._content)
                return self._content[start:end].strip()
        return ""

    def set_section(self, name: str, body: str) -> None:
        """Replace or append a named ## section."""
        body = body.strip()
        matches = list(_SECTION_RE.finditer(self._content))
        for i, m in enumerate(matches):
            if m.group(1).strip().lower() == name.lower():
                start = m.end()
                end = matches[i + 1].start() if i + 1 < len(matches) else len(self._content)
                self._content = (
                    self._content[: m.end()]
                    + "\n\n"
                    + body
                    + "\n\n"
                    + self._content[end:].lstrip("\n")
                )
                self._save()
                log.info(f"Updated section '{name}' in {self.path.name}")
                return

        # Section doesn't exist — append it
        self._content = self._content.rstrip() + f"\n\n## {name}\n\n{body}\n"
        self._save()
        log.info(f"Appended section '{name}' to {self.path.name}")

    def check_status_item(self, item_name: str) -> None:
        """Mark a status checkbox as checked."""
        def replacer(m: re.Match) -> str:
            if item_name.lower() in m.group(2).lower():
                return f"- [x] {m.group(2)}"
            return m.group(0)

        new_content = _STATUS_ITEM_RE.sub(replacer, self._content)
        if new_content != self._content:
            self._content = new_content
            self._save()
            log.info(f"Checked status item '{item_name}'")

    def _save(self) -> None:
        self.path.write_text(self._content, encoding="utf-8")
