"""
OmniAgent Engine — Agent Executor

Orchestrates the plan-route-execute cycle, integrating working memory,
hybrid local/cloud providers, tools, and real-time event broadcasting.
"""

from __future__ import annotations
import time
from omniagent.config import get_config, CloudProvider
from omniagent.events import AgentEvent, EventType, RoutingDecision, event_bus
from omniagent.memory import AgentMemory
from omniagent.planner import TaskPlanner
from omniagent.router import TaskRouter
from omniagent.providers.local import LocalProvider
from omniagent.providers.openai import OpenAIProvider
from omniagent.providers.gemini import GeminiProvider
from omniagent.tools import tool_registry

class AgentExecutor:
    """Main execution controller for OmniAgent."""

    def __init__(self, memory: AgentMemory | None = None) -> None:
        self.config = get_config()
        self.memory = memory or AgentMemory()
        self.router = TaskRouter()
        self.planner = TaskPlanner()

        # Initialize providers
        self.local_provider = LocalProvider()
        if self.config.cloud_provider == CloudProvider.GEMINI:
            self.cloud_provider = GeminiProvider()
        else:
            self.cloud_provider = OpenAIProvider()

    async def run(self, prompt: str) -> str:
        """Run an autonomous agent task loop."""
        start_time = time.time()
        self.memory.add_turn("user", prompt)

        event_bus.emit(AgentEvent(
            event_type=EventType.THINKING,
            message=f"Received goal: {prompt[:100]}...",
        ))

        # Step 1: Route the overall goal
        routing_res = self.router.route(prompt)

        # Step 2: Create execution plan
        plan = self.planner.plan(prompt)

        results = []
        for step in plan.steps:
            step.status = "running"
            event_bus.emit(AgentEvent(
                event_type=EventType.EXECUTING,
                message=f"Step {step.id}/{len(plan.steps)}: {step.description}",
                step_number=step.id,
                total_steps=len(plan.steps),
            ))

            # Select inference engine based on routing decision
            if routing_res.decision == RoutingDecision.LOCAL:
                provider = self.local_provider
            else:
                provider = self.cloud_provider if await self.cloud_provider.is_available() else self.local_provider

            ctx = self.memory.get_context_window(self.config.local_max_tokens)
            output = await provider.generate(prompt=step.description, messages=ctx)

            step.result = output
            step.status = "completed"
            results.append(output)
            self.memory.add_turn("assistant", output)

        final_response = "\n\n".join(results)
        elapsed_ms = (time.time() - start_time) * 1000

        event_bus.emit(AgentEvent(
            event_type=EventType.COMPLETED,
            message="Task completed successfully.",
            latency_ms=round(elapsed_ms, 1),
            data={"final_response": final_response},
        ))

        return final_response
