"""
OmniAgent Engine — Provider Base

Abstract base class for inference providers (local and cloud).
"""

from __future__ import annotations
from abc import ABC, abstractmethod

class BaseProvider(ABC):
    """Interface that all inference providers must implement."""

    name: str = "base"

    @abstractmethod
    async def generate(self, prompt: str, messages: list[dict] | None = None,
                       max_tokens: int = 1024, temperature: float = 0.7) -> str:
        """Generate a text completion."""
        ...

    @abstractmethod
    async def is_available(self) -> bool:
        """Check if this provider is ready to serve requests."""
        ...
