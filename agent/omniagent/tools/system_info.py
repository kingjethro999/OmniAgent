"""
OmniAgent Engine — System Info Tool
"""

from __future__ import annotations
import platform, os, sys
from omniagent.tools import BaseTool, ToolResult

class SystemInfoTool(BaseTool):
    name = "system_info"
    description = "Provides operating system, hardware, and runtime environment specifications."

    async def run(self, **kwargs) -> ToolResult:
        try:
            info = {
                "os": platform.system(),
                "os_release": platform.release(),
                "architecture": platform.machine(),
                "python_version": sys.version.split()[0],
                "cpu_count": os.cpu_count(),
            }
            formatted = "\n".join(f"- **{k}**: {v}" for k, v in info.items())
            return ToolResult(success=True, output=formatted, data=info)
        except Exception as e:
            return ToolResult(success=False, output="", error=str(e))
