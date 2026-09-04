"""
OmniAgent Engine — Sandboxed Code Runner Tool
"""

from __future__ import annotations
import sys, subprocess, tempfile
from pathlib import Path
from omniagent.tools import BaseTool, ToolResult

class CodeRunnerTool(BaseTool):
    name = "code_runner"
    description = "Executes Python code snippets in a local subprocess sandbox."

    async def run(self, code: str, timeout_sec: float = 10.0, **kwargs) -> ToolResult:
        try:
            with tempfile.NamedTemporaryFile(suffix=".py", mode="w", delete=False) as tmp:
                tmp.write(code)
                tmp_path = tmp.name

            res = subprocess.run(
                [sys.executable, tmp_path],
                capture_output=True, text=True, timeout=timeout_sec
            )
            Path(tmp_path).unlink(missing_ok=True)

            if res.returncode == 0:
                return ToolResult(success=True, output=res.stdout or "[Code executed cleanly with no stdout output]")
            else:
                return ToolResult(success=False, output=res.stdout, error=res.stderr)
        except subprocess.TimeoutExpired:
            return ToolResult(success=False, output="", error=f"Execution timed out after {timeout_sec}s.")
        except Exception as e:
            return ToolResult(success=False, output="", error=str(e))
