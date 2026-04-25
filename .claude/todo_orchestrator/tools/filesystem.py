from __future__ import annotations

import subprocess
from pathlib import Path
from typing import Any

from todo_orchestrator.tools.base import Tool


class ReadFileTool(Tool):
    def __init__(self) -> None:
        super().__init__(
            name="read_file",
            description="Read the contents of a file. Path is relative to project root.",
            input_schema={
                "type": "object",
                "properties": {"path": {"type": "string", "description": "Relative file path"}},
                "required": ["path"],
            },
        )

    def execute(self, project_root: Path, path: str, **_: Any) -> str:
        p = self._safe_path(project_root, path)
        if not p.exists():
            return f"ERROR: File not found: {path}"
        return p.read_text(encoding="utf-8")


class ListDirectoryTool(Tool):
    def __init__(self) -> None:
        super().__init__(
            name="list_directory",
            description="List files and directories at a given path (relative to project root).",
            input_schema={
                "type": "object",
                "properties": {"path": {"type": "string", "description": "Relative directory path", "default": "."}},
                "required": [],
            },
        )

    def execute(self, project_root: Path, path: str = ".", **_: Any) -> str:
        p = self._safe_path(project_root, path)
        if not p.exists():
            return f"ERROR: Directory not found: {path}"
        lines = []
        for item in sorted(p.iterdir()):
            suffix = "/" if item.is_dir() else ""
            lines.append(f"  {item.name}{suffix}")
        return "\n".join(lines) or "(empty)"


class FindFilesTool(Tool):
    def __init__(self) -> None:
        super().__init__(
            name="find_files",
            description="Find files matching a glob pattern within the project.",
            input_schema={
                "type": "object",
                "properties": {
                    "pattern": {"type": "string", "description": "Glob pattern, e.g. '**/*.cs'"},
                    "directory": {"type": "string", "description": "Directory to search in (default: project root)"},
                },
                "required": ["pattern"],
            },
        )

    def execute(self, project_root: Path, pattern: str, directory: str = ".", **_: Any) -> str:
        base = self._safe_path(project_root, directory)
        results = sorted(base.glob(pattern))
        if not results:
            return "No files found."
        return "\n".join(str(p.relative_to(project_root)) for p in results[:100])


class GrepCodebaseTool(Tool):
    def __init__(self) -> None:
        super().__init__(
            name="grep_codebase",
            description="Search for a pattern across the codebase.",
            input_schema={
                "type": "object",
                "properties": {
                    "pattern": {"type": "string", "description": "Search pattern (grep regex)"},
                    "file_pattern": {"type": "string", "description": "File filter e.g. '*.cs' (optional)"},
                    "case_sensitive": {"type": "boolean", "default": True},
                },
                "required": ["pattern"],
            },
        )

    def execute(
        self,
        project_root: Path,
        pattern: str,
        file_pattern: str = "",
        case_sensitive: bool = True,
        **_: Any,
    ) -> str:
        cmd = ["grep", "-rn", "--include=*.md", "--include=*.cs", "--include=*.razor",
               "--include=*.ts", "--include=*.tsx", "--include=*.js", "--include=*.json"]
        if file_pattern:
            cmd = ["grep", "-rn", f"--include={file_pattern}"]
        if not case_sensitive:
            cmd.append("-i")
        cmd += [pattern, str(project_root)]
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=30)
        output = result.stdout.strip()
        if not output:
            return "No matches found."
        lines = output.splitlines()
        # Make paths relative
        rel_lines = [l.replace(str(project_root) + "/", "") for l in lines]
        return "\n".join(rel_lines[:80])


class WriteFileTool(Tool):
    def __init__(self) -> None:
        super().__init__(
            name="write_file",
            description="Write content to a file. Creates the file if it doesn't exist. Path is relative to project root.",
            input_schema={
                "type": "object",
                "properties": {
                    "path": {"type": "string", "description": "Relative file path"},
                    "content": {"type": "string", "description": "File content"},
                },
                "required": ["path", "content"],
            },
        )

    def execute(self, project_root: Path, path: str, content: str, **_: Any) -> str:
        p = self._safe_path(project_root, path)
        p.parent.mkdir(parents=True, exist_ok=True)
        p.write_text(content, encoding="utf-8")
        return f"Written: {path} ({len(content)} chars)"


class EditFileTool(Tool):
    def __init__(self) -> None:
        super().__init__(
            name="edit_file",
            description="Replace an exact string in a file with new content.",
            input_schema={
                "type": "object",
                "properties": {
                    "path": {"type": "string", "description": "Relative file path"},
                    "old_string": {"type": "string", "description": "Exact string to replace"},
                    "new_string": {"type": "string", "description": "Replacement string"},
                },
                "required": ["path", "old_string", "new_string"],
            },
        )

    def execute(self, project_root: Path, path: str, old_string: str, new_string: str, **_: Any) -> str:
        p = self._safe_path(project_root, path)
        if not p.exists():
            return f"ERROR: File not found: {path}"
        content = p.read_text(encoding="utf-8")
        if old_string not in content:
            return f"ERROR: String not found in {path}"
        new_content = content.replace(old_string, new_string, 1)
        p.write_text(new_content, encoding="utf-8")
        return f"Edited: {path}"
