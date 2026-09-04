"""
OmniAgent Engine — Agent Configuration

Centralized settings loaded from environment variables with sensible defaults.
All config values are validated at startup via Pydantic.
"""

from __future__ import annotations

import os
from pathlib import Path
from enum import Enum

from pydantic import BaseModel, Field
from dotenv import load_dotenv


# Load .env from project root (two levels up from this file)
_project_root = Path(__file__).resolve().parent.parent.parent
load_dotenv(_project_root / ".env")


class CloudProvider(str, Enum):
    OPENAI = "openai"
    GEMINI = "gemini"


class AgentConfig(BaseModel):
    """Immutable configuration snapshot for an OmniAgent session."""

    # ── Cloud Provider Settings ──
    cloud_provider: CloudProvider = Field(
        default_factory=lambda: CloudProvider(
            os.getenv("OMNI_CLOUD_PROVIDER", "openai")
        )
    )
    cloud_model: str = Field(
        default_factory=lambda: os.getenv("OMNI_CLOUD_MODEL", "gpt-4o")
    )
    openai_api_key: str = Field(
        default_factory=lambda: os.getenv("OPENAI_API_KEY", "")
    )
    gemini_api_key: str = Field(
        default_factory=lambda: os.getenv("GEMINI_API_KEY", "")
    )

    # ── Local Model Settings ──
    local_model_path: str = Field(
        default_factory=lambda: os.getenv(
            "OMNI_LOCAL_MODEL_PATH", "./models/phi-4-mini.gguf"
        )
    )
    local_max_tokens: int = Field(
        default_factory=lambda: int(os.getenv("OMNI_LOCAL_MAX_TOKENS", "2048"))
    )

    # ── Routing ──
    complexity_threshold: float = Field(
        default_factory=lambda: float(
            os.getenv("OMNI_COMPLEXITY_THRESHOLD", "0.6")
        )
    )

    # ── Agent Behaviour ──
    max_steps: int = Field(
        default_factory=lambda: int(os.getenv("OMNI_MAX_STEPS", "10"))
    )
    verbose: bool = Field(
        default_factory=lambda: os.getenv("OMNI_VERBOSE", "true").lower()
        == "true"
    )

    # ── Dashboard ──
    dashboard_port: int = Field(
        default_factory=lambda: int(os.getenv("DASHBOARD_PORT", "3000"))
    )
    dashboard_ws_port: int = Field(
        default_factory=lambda: int(os.getenv("DASHBOARD_WS_PORT", "3001"))
    )

    @property
    def has_cloud_key(self) -> bool:
        """Check if the active cloud provider's API key is configured."""
        if self.cloud_provider == CloudProvider.OPENAI:
            return bool(self.openai_api_key) and not self.openai_api_key.startswith("sk-your")
        return bool(self.gemini_api_key) and not self.gemini_api_key.startswith("your-")


# Singleton instance
_config: AgentConfig | None = None


def get_config() -> AgentConfig:
    """Return the global config singleton, creating it on first access."""
    global _config
    if _config is None:
        _config = AgentConfig()
    return _config


def reload_config() -> AgentConfig:
    """Force-reload config from environment."""
    global _config
    _config = AgentConfig()
    return _config
