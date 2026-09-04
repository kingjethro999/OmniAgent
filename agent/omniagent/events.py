"""
OmniAgent Engine — Event System

Typed event models emitted during agent execution, consumed by the
WebSocket dashboard for real-time visualization.
"""

from __future__ import annotations

import time
import uuid
from enum import Enum
from typing import Any

from pydantic import BaseModel, Field


class EventType(str, Enum):
    """Categories of events emitted during agent execution."""
    THINKING = "THINKING"
    PLANNING = "PLANNING"
    ROUTING = "ROUTING"
    EXECUTING = "EXECUTING"
    TOOL_CALL = "TOOL_CALL"
    TOOL_RESULT = "TOOL_RESULT"
    COMPLETED = "COMPLETED"
    ERROR = "ERROR"
    SYSTEM = "SYSTEM"


class RoutingDecision(str, Enum):
    """Where a task was routed for inference."""
    LOCAL = "LOCAL"
    CLOUD = "CLOUD"


class AgentEvent(BaseModel):
    """A single event in the agent execution lifecycle."""

    id: str = Field(default_factory=lambda: uuid.uuid4().hex[:12])
    timestamp: float = Field(default_factory=time.time)
    event_type: EventType
    agent_id: str = "default"

    # ── Payload Fields (optional, depends on event_type) ──
    message: str = ""
    data: dict[str, Any] = Field(default_factory=dict)

    # Routing-specific
    routing_decision: RoutingDecision | None = None
    complexity_score: float | None = None

    # Execution-specific
    step_number: int | None = None
    total_steps: int | None = None

    # Metrics
    tokens_used: int | None = None
    latency_ms: float | None = None
    provider: str | None = None

    def to_ws_payload(self) -> dict[str, Any]:
        """Serialize to a JSON-safe dict for WebSocket transmission."""
        payload = self.model_dump(exclude_none=True)
        payload["event_type"] = self.event_type.value
        if self.routing_decision:
            payload["routing_decision"] = self.routing_decision.value
        return payload


class EventBus:
    """
    Simple in-process event bus. Listeners can subscribe to receive
    AgentEvent instances in real time.
    """

    def __init__(self) -> None:
        self._listeners: list[callable] = []
        self._history: list[AgentEvent] = []

    def subscribe(self, listener: callable) -> None:
        """Register a callback that receives AgentEvent instances."""
        self._listeners.append(listener)

    def unsubscribe(self, listener: callable) -> None:
        """Remove a previously registered listener."""
        self._listeners = [ln for ln in self._listeners if ln is not listener]

    def emit(self, event: AgentEvent) -> None:
        """Broadcast an event to all registered listeners."""
        self._history.append(event)
        for listener in self._listeners:
            try:
                listener(event)
            except Exception:
                pass  # Dashboard disconnects should not crash the agent

    @property
    def history(self) -> list[AgentEvent]:
        """Return all events emitted during this session."""
        return list(self._history)

    def clear(self) -> None:
        """Clear event history."""
        self._history.clear()


# Global event bus singleton
event_bus = EventBus()
