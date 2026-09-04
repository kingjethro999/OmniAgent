"""
OmniAgent Engine — File Operations Tool
"""

from __future__ import annotations
import os, glob
from pathlib import Path
from omniagent.tools import BaseTool, ToolResult

class FileOpsTool(BaseTool):
    name = "file_ops"
    description = "Read, write, search, or list files locally on the machine."

    async def run(self, action: str, path: str, content: str | None = None, pattern: str | None = None, **kwargs) -> ToolResult:
        try:
            target_path = Path(path).expanduser().resolve()

            if action == "read":
                if not target_path.exists() or not target_path.is_file():
                    return ToolResult(success=False, output="", error=f"File not found: {path}")
                text = target_path.read_text(encoding="utf-8", errors="replace")
                return ToolResult(success=True, output=text)

            elif action == "write":
                target_path.parent.mkdir(parents=True, exist_ok=True)
                target_path.write_text(content or "", encoding="utf-8")
                return ToolResult(success=True, output=f"Successfully wrote {len(content or '')} bytes to {path}")

            elif action == "list":
                if not target_path.exists() or not target_path.is_dir():
                    return ToolResult(success=False, output="", error=f"Directory not found: {path}")
                items = [f"{'[DIR] ' if p.is_dir() else '[FILE]'} {p.name}" for p in target_path.iterdir()]
                return ToolResult(success=True, output="\n".join(items))

            elif action == "search":
                pat = pattern or "*"
                matches = glob.glob(str(target_path / "**" / pat), recursive=True)
                return ToolResult(success=True, output="\n".join(matches[:50]))

            else:
                return ToolResult(success=False, output="", error=f"Unknown action: {action}")

        except Exception as e:
            return ToolResult(success=False, output="", error=str(e))
