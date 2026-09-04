"""
OmniAgent Engine — Tools Registry & Base Class

Defines the BaseTool abstract interface and ToolRegistry for agent tool discovery and execution.
"""

from __future__ import annotations
from abc import ABC, abstractmethod
from typing import Any
from pydantic import BaseModel

class ToolResult(BaseModel):
    success: bool
    output: str
    error: str | None = None
    data: dict[str, Any] | None = None

class BaseTool(ABC):
    """Abstract base class for all OmniAgent tools."""
    name: str
    description: str

    @abstractmethod
    async def run(self, **kwargs) -> ToolResult:
        """Execute the tool with the provided keyword arguments."""
        ...

class ToolRegistry:
    """Central registry holding available tools for agent invocation."""

    def __init__(self) -> None:
        self._tools: dict[str, BaseTool] = {}

    def register(self, tool: BaseTool) -> None:
        self._tools[tool.name] = tool

    def get(self, name: str) -> BaseTool | None:
        return self._tools.get(name)

    def list_tools(self) -> list[dict[str, str]]:
        return [{"name": t.name, "description": t.description} for t in self._tools.values()]

    async def execute(self, tool_name: str, **kwargs) -> ToolResult:
        tool = self.get(tool_name)
        if not tool:
            return ToolResult(success=False, output="", error=f"Tool '{tool_name}' not found.")
        try:
            return await tool.run(**kwargs)
        except Exception as e:
            return ToolResult(success=False, output="", error=str(e))

tool_registry = ToolRegistry()

def _register_defaults():
    from omniagent.tools.file_ops import FileOpsTool
    from omniagent.tools.web_search import WebSearchTool
    from omniagent.tools.code_runner import CodeRunnerTool
    from omniagent.tools.system_info import SystemInfoTool

    tool_registry.register(FileOpsTool())
    tool_registry.register(WebSearchTool())
    tool_registry.register(CodeRunnerTool())
    tool_registry.register(SystemInfoTool())

_register_defaults()
