"""
OmniAgent Engine — Working Memory

Key-value working memory with conversation history tracking and
context window management for both local and cloud providers.
"""

from __future__ import annotations

import time
from typing import Any

from pydantic import BaseModel, Field


class MemoryEntry(BaseModel):
    """A single entry in working memory."""
    key: str
    value: Any
    created_at: float = Field(default_factory=time.time)
    ttl: float | None = None  # seconds, None = permanent

    @property
    def is_expired(self) -> bool:
        if self.ttl is None:
            return False
        return (time.time() - self.created_at) > self.ttl


class ConversationTurn(BaseModel):
    """A single turn in the conversation history."""
    role: str  # "user", "assistant", "system", "tool"
    content: str
    timestamp: float = Field(default_factory=time.time)
    metadata: dict[str, Any] = Field(default_factory=dict)


class AgentMemory:
    """
    Working memory and conversation history for an OmniAgent session.

    Provides:
    - Key-value scratchpad with optional TTL expiration
    - Ordered conversation history with role tracking
    - Context window truncation for token-limited providers
    """

    def __init__(self, max_history: int = 50) -> None:
        self._store: dict[str, MemoryEntry] = {}
        self._history: list[ConversationTurn] = []
        self._max_history = max_history

    # ── Key-Value Store ──

    def set(self, key: str, value: Any, ttl: float | None = None) -> None:
        """Store a value in working memory."""
        self._store[key] = MemoryEntry(key=key, value=value, ttl=ttl)

    def get(self, key: str, default: Any = None) -> Any:
        """Retrieve a value, returning default if missing or expired."""
        entry = self._store.get(key)
        if entry is None:
            return default
        if entry.is_expired:
            del self._store[key]
            return default
        return entry.value

    def delete(self, key: str) -> bool:
        """Remove a key from working memory. Returns True if it existed."""
        return self._store.pop(key, None) is not None

    def keys(self) -> list[str]:
        """Return all non-expired keys."""
        self._gc()
        return list(self._store.keys())

    def _gc(self) -> None:
        """Garbage-collect expired entries."""
        expired = [k for k, v in self._store.items() if v.is_expired]
        for k in expired:
            del self._store[k]

    # ── Conversation History ──

    def add_turn(
        self,
        role: str,
        content: str,
        metadata: dict[str, Any] | None = None,
    ) -> None:
        """Append a conversation turn."""
        self._history.append(
            ConversationTurn(
                role=role,
                content=content,
                metadata=metadata or {},
            )
        )
        # Enforce max history length
        if len(self._history) > self._max_history:
            self._history = self._history[-self._max_history :]

    def get_history(
        self, last_n: int | None = None
    ) -> list[ConversationTurn]:
        """Return conversation history, optionally limited to last N turns."""
        if last_n is None:
            return list(self._history)
        return list(self._history[-last_n:])

    def get_messages(
        self, last_n: int | None = None
    ) -> list[dict[str, str]]:
        """Return history as a list of {role, content} dicts for LLM APIs."""
        turns = self.get_history(last_n)
        return [{"role": t.role, "content": t.content} for t in turns]

    def get_context_window(self, max_tokens: int = 2048) -> list[dict[str, str]]:
        """
        Build a context window that fits within the token budget.
        Uses a rough 4-chars-per-token estimate for fast trimming.
        """
        messages = self.get_messages()
        result = []
        total_chars = 0
        char_budget = max_tokens * 4

        # Always include the system message if present
        for msg in messages:
            if msg["role"] == "system":
                result.append(msg)
                total_chars += len(msg["content"])
                break

        # Fill from most recent backwards
        for msg in reversed(messages):
            if msg["role"] == "system":
                continue
            msg_chars = len(msg["content"])
            if total_chars + msg_chars > char_budget:
                break
            result.insert(-1 if result else 0, msg)
            total_chars += msg_chars

        return result

    def clear(self) -> None:
        """Reset all memory."""
        self._store.clear()
        self._history.clear()

    def summary(self) -> dict[str, Any]:
        """Return a snapshot of memory state for debugging."""
        self._gc()
        return {
            "kv_entries": len(self._store),
            "kv_keys": list(self._store.keys()),
            "conversation_turns": len(self._history),
            "last_role": self._history[-1].role if self._history else None,
        }
