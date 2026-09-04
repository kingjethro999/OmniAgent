"""
OmniAgent Engine — Google Gemini Cloud Provider

Async httpx-based adapter for the Gemini GenerateContent API.
"""

from __future__ import annotations
import time
import httpx
from omniagent.providers import BaseProvider
from omniagent.config import get_config
from omniagent.events import AgentEvent, EventType, event_bus

_API_URL = "https://generativelanguage.googleapis.com/v1beta/models"

class GeminiProvider(BaseProvider):
    name = "gemini"

    def __init__(self):
        cfg = get_config()
        self._key = cfg.gemini_api_key
        self._model = "gemini-2.0-flash"

    async def generate(self, prompt: str, messages: list[dict] | None = None,
                       max_tokens: int = 1024, temperature: float = 0.7) -> str:
        if not self._key:
            return "[Gemini] API key not configured. Set GEMINI_API_KEY in .env"
        url = f"{_API_URL}/{self._model}:generateContent?key={self._key}"
        parts = [{"text": m["content"]} for m in (messages or [{"role":"user","content":prompt}])]
        start = time.time()
        async with httpx.AsyncClient(timeout=60) as client:
            resp = await client.post(url, json={
                "contents": [{"parts": parts}],
                "generationConfig": {"maxOutputTokens": max_tokens, "temperature": temperature},
            })
            resp.raise_for_status()
            data = resp.json()
        latency = (time.time() - start) * 1000
        content = data["candidates"][0]["content"]["parts"][0]["text"]
        event_bus.emit(AgentEvent(
            event_type=EventType.EXECUTING, message="Cloud inference (Gemini) completed",
            provider="gemini", latency_ms=round(latency, 1),
        ))
        return content

    async def is_available(self) -> bool:
        return bool(self._key) and not self._key.startswith("your-")
