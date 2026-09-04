"""
OmniAgent Engine — Task Planner

Decomposes complex user goals into ordered sub-tasks with
dependency tracking and local/cloud routing annotations.
"""

from __future__ import annotations
from dataclasses import dataclass, field
from omniagent.events import AgentEvent, EventType, event_bus

@dataclass
class SubTask:
    id: int
    description: str
    depends_on: list[int] = field(default_factory=list)
    tool: str | None = None
    status: str = "pending"  # pending | running | completed | failed
    result: str | None = None

@dataclass
class Plan:
    goal: str
    steps: list[SubTask]
    @property
    def is_complete(self) -> bool:
        return all(s.status in ("completed","failed") for s in self.steps)
    @property
    def next_step(self) -> SubTask | None:
        for s in self.steps:
            if s.status == "pending":
                deps_met = all(
                    self.steps[d-1].status == "completed"
                    for d in s.depends_on if d <= len(self.steps)
                )
                if deps_met:
                    return s
        return None

class TaskPlanner:
    """Decomposes a user goal into an ordered list of sub-tasks."""

    _DECOMPOSITION_PATTERNS = [
        (r"\b(and then|then|after that|next|finally)\b",),
        (r"^\s*\d+[\.\)]\s",),
        (r"\b(first|second|third|lastly)\b",),
    ]

    def plan(self, goal: str) -> Plan:
        import re
        event_bus.emit(AgentEvent(
            event_type=EventType.PLANNING,
            message=f"Planning: {goal[:80]}...",
        ))
        # Try splitting on explicit numbered steps
        numbered = re.split(r"\n\s*\d+[\.\)]\s*", goal)
        if len(numbered) > 2:
            steps = [
                SubTask(id=i+1, description=s.strip(), depends_on=[i] if i > 0 else [])
                for i, s in enumerate(numbered) if s.strip()
            ]
            return Plan(goal=goal, steps=steps)

        # Try splitting on sequence words
        seq_parts = re.split(r"\b(?:then|after that|next|finally)\b", goal, flags=re.I)
        if len(seq_parts) > 1:
            steps = [
                SubTask(id=i+1, description=s.strip(), depends_on=[i] if i > 0 else [])
                for i, s in enumerate(seq_parts) if s.strip()
            ]
            return Plan(goal=goal, steps=steps)

        # Single-step plan for simple tasks
        return Plan(goal=goal, steps=[SubTask(id=1, description=goal)])
