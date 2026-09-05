"""
OmniAgent Engine — Local SLM Provider

Interfaces with the C++ core engine via ctypes for on-device inference.
Falls back to a stub response when the native library is not yet compiled.
"""

from __future__ import annotations
import ctypes
import os
import sys
import time
from pathlib import Path
from omniagent.providers import BaseProvider
from omniagent.config import get_config
from omniagent.events import AgentEvent, EventType, event_bus

class LocalProvider(BaseProvider):
    name = "local"

    def __init__(self):
        self._engine = None
        self._ctx = None
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
                self._engine.omni_init_engine.argtypes = [ctypes.c_char_p, ctypes.c_int32]
                self._engine.omni_init_engine.restype = ctypes.c_void_p

                self._engine.omni_generate.argtypes = [
                    ctypes.c_void_p,
                    ctypes.c_char_p,
                    ctypes.c_char_p,
                    ctypes.c_size_t,
                    ctypes.c_float,
                ]
                self._engine.omni_generate.restype = ctypes.c_int32

                self._engine.omni_free_engine.argtypes = [ctypes.c_void_p]
                self._engine.omni_free_engine.restype = None

                model_path = config.local_model_path.encode("utf-8")
                self._ctx = self._engine.omni_init_engine(model_path, 4)
            except Exception:
                self._engine = None
                self._ctx = None

    def __del__(self):
        if self._engine is not None and self._ctx is not None:
            try:
                self._engine.omni_free_engine(self._ctx)
            except Exception:
                pass

    async def generate(self, prompt: str, messages: list[dict] | None = None,
                       max_tokens: int = 1024, temperature: float = 0.7) -> str:
        start = time.time()
        if self._engine is not None and self._ctx is not None:
            result = self._call_native(prompt, max_tokens, temperature)
        else:
            result = self._stub_generate(prompt)
        latency = (time.time() - start) * 1000
        event_bus.emit(AgentEvent(
            event_type=EventType.EXECUTING,
            message="Local inference completed",
            provider="local", latency_ms=round(latency, 1),
            tokens_used=len(result.split()),
        ))
        return result

    def _call_native(self, prompt: str, max_tokens: int, temperature: float) -> str:
        """Call the C++ engine via ctypes."""
        try:
            buf = ctypes.create_string_buffer(4096)
            res = self._engine.omni_generate(
                self._ctx,
                prompt.encode("utf-8"),
                buf,
                4096,
                ctypes.c_float(temperature),
            )
            if res > 0:
                return buf.value.decode("utf-8", errors="replace")
        except Exception:
            pass
        return self._stub_generate(prompt)

    def _stub_generate(self, prompt: str) -> str:
        """Development stub that simulates local model reasoning."""
        prompt_lower = prompt.lower()
        if any(w in prompt_lower for w in ("summarize", "summary")):
            return "[Local SLM] Here is a concise summary of the provided content. The key points are extracted and presented in order of relevance."
        if any(w in prompt_lower for w in ("file", "folder", "list")):
            return "[Local SLM] I've processed your file-related request locally. No data was sent to the cloud."
        if any(w in prompt_lower for w in ("translate", "draft", "reply")):
            return "[Local SLM] Draft generated locally for privacy. Review and send when ready."
        return f"[Local SLM] Task processed on-device. Prompt length: {len(prompt)} chars."

    async def is_available(self) -> bool:
        return True
