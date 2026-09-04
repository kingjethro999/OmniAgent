"""
OmniAgent Engine — OpenAI Cloud Provider

Async httpx-based adapter for the OpenAI Chat Completions API.
"""

from __future__ import annotations
import time
import httpx
from omniagent.providers import BaseProvider
from omniagent.config import get_config
from omniagent.events import AgentEvent, EventType, event_bus

_API_URL = "https://api.openai.com/v1/chat/completions"

class OpenAIProvider(BaseProvider):
    name = "openai"

    def __init__(self):
        cfg = get_config()
        self._key = cfg.openai_api_key
        self._model = cfg.cloud_model

    async def generate(self, prompt: str, messages: list[dict] | None = None,
                       max_tokens: int = 1024, temperature: float = 0.7) -> str:
        if not self._key:
            return "[OpenAI] API key not configured. Set OPENAI_API_KEY in .env"
        msgs = messages or [{"role": "user", "content": prompt}]
        start = time.time()
        async with httpx.AsyncClient(timeout=60) as client:
            resp = await client.post(_API_URL, json={
                "model": self._model, "messages": msgs,
                "max_tokens": max_tokens, "temperature": temperature,
            }, headers={"Authorization": f"Bearer {self._key}", "Content-Type": "application/json"})
            resp.raise_for_status()
            data = resp.json()
        latency = (time.time() - start) * 1000
        content = data["choices"][0]["message"]["content"]
        usage = data.get("usage", {})
        event_bus.emit(AgentEvent(
            event_type=EventType.EXECUTING, message="Cloud inference (OpenAI) completed",
            provider="openai", latency_ms=round(latency, 1),
            tokens_used=usage.get("total_tokens"),
        ))
        return content

    async def is_available(self) -> bool:
        return bool(self._key) and not self._key.startswith("sk-your")
