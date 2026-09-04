"""
OmniAgent Engine — Local SLM Provider

Interfaces with the C++ core engine via ctypes for on-device inference.
Falls back to a stub response when the native library is not yet compiled.
"""

from __future__ import annotations
import ctypes, os, sys, time
from pathlib import Path
from omniagent.providers import BaseProvider
from omniagent.config import get_config
from omniagent.events import AgentEvent, EventType, event_bus

class LocalProvider(BaseProvider):
    name = "local"

    def __init__(self):
        self._engine = None
        self._load_native()

    def _load_native(self):
        """Attempt to load the C++ shared library."""
        config = get_config()
        lib_names = {
            "linux": "libomni_engine.so",
            "darwin": "libomni_engine.dylib",
            "win32": "omni_engine.dll",
        }
        lib_name = lib_names.get(sys.platform, "libomni_engine.so")
        lib_path = Path(__file__).resolve().parent.parent.parent.parent / "core" / "build" / lib_name
        if lib_path.exists():
            try:
                self._engine = ctypes.CDLL(str(lib_path))
            except OSError:
                self._engine = None

    async def generate(self, prompt: str, messages: list[dict] | None = None,
                       max_tokens: int = 1024, temperature: float = 0.7) -> str:
        start = time.time()
        if self._engine is not None:
            # Real C++ engine call (Phase 2)
            result = self._call_native(prompt, max_tokens, temperature)
        else:
            # Stub: echo-based response for development
            result = self._stub_generate(prompt)
        latency = (time.time() - start) * 1000
        event_bus.emit(AgentEvent(
            event_type=EventType.EXECUTING,
            message=f"Local inference completed",
            provider="local", latency_ms=round(latency, 1),
            tokens_used=len(result.split()),
        ))
        return result

    def _call_native(self, prompt: str, max_tokens: int, temperature: float) -> str:
        """Call the C++ engine via ctypes."""
        # This will be implemented when the C++ core is compiled
        return "[Native engine response placeholder]"

    def _stub_generate(self, prompt: str) -> str:
        """Development stub that simulates local model reasoning."""
        prompt_lower = prompt.lower()
        if any(w in prompt_lower for w in ("summarize", "summary")):
            return f"[Local SLM] Here is a concise summary of the provided content. The key points are extracted and presented in order of relevance."
        if any(w in prompt_lower for w in ("file", "folder", "list")):
            return f"[Local SLM] I've processed your file-related request locally. No data was sent to the cloud."
        if any(w in prompt_lower for w in ("translate", "draft", "reply")):
            return f"[Local SLM] Draft generated locally for privacy. Review and send when ready."
        return f"[Local SLM] Task processed on-device. Prompt length: {len(prompt)} chars."

    async def is_available(self) -> bool:
        return True  # Stub is always available
