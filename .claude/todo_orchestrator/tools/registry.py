from __future__ import annotations

from todo_orchestrator.tools.base import Tool
from todo_orchestrator.tools.filesystem import (
    EditFileTool,
    FindFilesTool,
    GrepCodebaseTool,
    ListDirectoryTool,
    ReadFileTool,
    WriteFileTool,
)
from todo_orchestrator.tools.git_tools import GitAddTool, GitCommitTool, GitStatusTool
from todo_orchestrator.tools.process_tools import MoveFileTool, RunFormatterTool
from todo_orchestrator.tools.todo_tools import AskUserTool, UpdateTodoStatusTool, WriteTodoSectionTool


def build_tool_registry(phase: str, todo_file: object) -> list[Tool]:
    """Return the list of tools allowed for a given phase."""
    read_only = [
        ReadFileTool(),
        ListDirectoryTool(),
        FindFilesTool(),
        GrepCodebaseTool(),
    ]
    todo_write = [
        WriteTodoSectionTool(todo_file),
        UpdateTodoStatusTool(todo_file),
    ]
    file_write = [
        WriteFileTool(),
        EditFileTool(),
    ]
    git = [GitStatusTool(), GitAddTool(), GitCommitTool()]
    finish = [MoveFileTool(), RunFormatterTool()] + git

    registry: dict[str, list[Tool]] = {
        "research": read_only + todo_write,
        "plan": read_only + todo_write + [AskUserTool()],
        "implement": read_only + file_write + todo_write,
        "review": read_only + todo_write,
        "release_notes": read_only + [WriteFileTool(), EditFileTool()] + todo_write,
        "finish": finish,
    }
    return registry.get(phase, read_only)
