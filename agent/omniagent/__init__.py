"""
OmniAgent Engine — Hybrid Local/Cloud Edge Agent Framework

A lightweight, cross-platform framework for building autonomous AI agents
that operate locally on edge devices with intelligent cloud offloading.
"""

__version__ = "0.2.1"
__author__ = "Japheth"

from omniagent.router import TaskRouter
from omniagent.planner import TaskPlanner
from omniagent.executor import AgentExecutor
from omniagent.memory import AgentMemory
from omniagent.ide_hook import IDEHookServer

__all__ = [
    "TaskRouter",
    "TaskPlanner",
    "AgentExecutor",
    "AgentMemory",
    "IDEHookServer",
]
