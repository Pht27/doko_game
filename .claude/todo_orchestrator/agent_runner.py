from __future__ import annotations

import json
from pathlib import Path
from typing import Any

import anthropic

from todo_orchestrator.logger import get_logger
from todo_orchestrator.tools.base import Tool

log = get_logger(__name__)

_MAX_ITERATIONS = 40


def run_agent(
    *,
    system_prompt: str,
    user_prompt: str,
    model: str,
    tools: list[Tool],
    project_root: Path,
    api_key: str,
    max_tokens: int = 8096,
    dry_run: bool = False,
) -> str:
    """Run an agentic loop until the model stops with end_turn. Returns final text."""
    if dry_run:
        log.info(f"[DRY RUN] Would call {model} with {len(tools)} tools")
        log.info(f"[DRY RUN] System: {system_prompt[:200]}...")
        log.info(f"[DRY RUN] User: {user_prompt[:200]}...")
        return "(dry run — no API call made)"

    client = anthropic.Anthropic(api_key=api_key)
    api_tools = [t.to_api_dict() for t in tools]
    tool_map = {t.name: t for t in tools}

    messages: list[dict] = [{"role": "user", "content": user_prompt}]

    for iteration in range(_MAX_ITERATIONS):
        kwargs: dict[str, Any] = {
            "model": model,
            "max_tokens": max_tokens,
            "system": system_prompt,
            "messages": messages,
        }
        if api_tools:
            kwargs["tools"] = api_tools

        log.debug(f"API call #{iteration + 1} ({model})")
        response = client.messages.create(**kwargs)

        # Collect text from this turn
        text_parts = [b.text for b in response.content if hasattr(b, "text") and b.text]
        if text_parts:
            log.info("Agent: " + " ".join(text_parts)[:300])

        if response.stop_reason == "end_turn":
            return "\n".join(text_parts)

        if response.stop_reason != "tool_use":
            log.warning(f"Unexpected stop_reason: {response.stop_reason}")
            return "\n".join(text_parts)

        # Process tool calls
        tool_results = []
        for block in response.content:
            if block.type != "tool_use":
                continue
            tool_name = block.name
            tool_input = block.input or {}
            log.info(f"  → Tool: {tool_name}({_summarize(tool_input)})")

            if tool_name not in tool_map:
                result_content = f"ERROR: Unknown tool '{tool_name}'"
            else:
                try:
                    result_content = tool_map[tool_name].execute(project_root, **tool_input)
                except PermissionError as e:
                    result_content = f"PERMISSION DENIED: {e}"
                except Exception as e:
                    result_content = f"ERROR: {e}"

            log.debug(f"     ↳ {str(result_content)[:200]}")
            tool_results.append({
                "type": "tool_result",
                "tool_use_id": block.id,
                "content": str(result_content),
            })

        messages.append({"role": "assistant", "content": response.content})
        messages.append({"role": "user", "content": tool_results})

    log.warning(f"Reached max iterations ({_MAX_ITERATIONS})")
    return "(max iterations reached)"


def _summarize(d: dict) -> str:
    parts = []
    for k, v in d.items():
        v_str = str(v)
        parts.append(f"{k}={v_str[:40]!r}" if len(v_str) > 40 else f"{k}={v_str!r}")
    return ", ".join(parts)
