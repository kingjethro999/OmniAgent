<p align="center">
  <img src="assets/omniagent_logo.png" alt="OmniAgent Logo" width="220" />
</p>

# OmniAgent Engine

**A Hybrid Local/Cloud Edge Agent Framework with On-Device SLM Inference & IDE Hook (MCP)**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Python: 3.10+](https://img.shields.io/badge/Python-3.10%2B-brightgreen.svg)](https://python.org)
[![Next.js: 14](https://img.shields.io/badge/Dashboard-Next.js%2014-black.svg)](https://nextjs.org)
[![GGUF: llama.cpp](https://img.shields.io/badge/Inference-GGUF%20%2F%20llama.cpp-orange.svg)](https://github.com/ggerganov/llama.cpp)
[![Protocol: MCP](https://img.shields.io/badge/MCP-Compatible%20(2024--11--05)-purple.svg)](https://modelcontextprotocol.io)

OmniAgent Engine is a lightweight, high-performance edge framework for running autonomous AI agents on local devices. It features an intelligent **hybrid router** that executes routine tasks locally on quantized Small Language Models (SLMs like Phi-4-mini, Qwen 2.5, or Llama 3.2), seamlessly offloading only heavy multi-step reasoning to cloud LLMs (OpenAI GPT-4o or Google Gemini).

---

## 📑 Table of Contents

- [Key Features](#-key-features)
- [Target Use Cases](#-target-use-cases)
- [Architecture Overview](#-architecture-overview)
- [Project Layout](#-project-layout)
- [Prerequisites](#-prerequisites)
- [Quick Start](#-quick-start)
- [Model Setup & Management](#-model-setup--management)
- [Using OmniAgent](#-using-omniagent)
  - [1. CLI Quick Execution](#1-cli-quick-execution)
  - [2. Interactive Terminal REPL](#2-interactive-terminal-repl)
  - [3. Python SDK](#3-python-sdk)
  - [4. Real-Time Web Dashboard](#4-real-time-web-dashboard)
- [IDE Hook & Model Context Protocol (MCP) Server](#-ide-hook--model-context-protocol-mcp-server)
  - [Starting the IDE Hook](#starting-the-ide-hook)
  - [Connecting to Cursor / VS Code / Claude](#connecting-to-cursor--vs-code--claude)
  - [API Endpoints & JSON-RPC Examples](#api-endpoints--json-rpc-examples)
- [Configuration Reference (.env)](#-configuration-reference-env)
- [Roadmap](#-roadmap)
- [License](#-license)

---

## 🔑 Key Features

- **Hybrid Edge/Cloud Routing**: Dynamic complexity scoring (0.0 to 1.0) based on keyword matching, token length, grammatical structure, and mathematical reasoning. Automatically directs privacy-sensitive or routine tasks to local SLMs and complex reasoning to cloud LLMs.
- **Privacy-First On-Device Execution**: Files, local logs, sensitive code, and system operations stay on your device without leaving your network.
- **GGUF Small Language Models**: Out-of-the-box support for Microsoft Phi-4-mini (3.8B), Qwen 2.5 (3B / 7B), and Llama 3.2 (3B) in 4-bit quantized GGUF format.
- **Built-in Tool Ecosystem**: Extensible tool registry featuring sandboxed code execution (`code_runner`), local file manipulation (`file_ops`), search (`web_search`), and hardware telemetry (`system_info`).
- **IDE Hook & MCP Bridge**: Embedded JSON-RPC server implementing the Model Context Protocol (MCP) standard to empower IDEs (Cursor, VS Code, JetBrains, Antigravity) with local code auditing and routing.
- **Live Monitoring Dashboard**: Next.js web application with Server-Sent Events (SSE) streaming live agent thoughts, plan step status, latency, and estimated cost savings.

---

## 🎯 Target Use Cases

| Use Case | How OmniAgent Solves It |
|---|---|
| **Privacy-Preserving Code Audits** | Developers auditing proprietary or confidential code can run vulnerability assessments and AST linting entirely on-device via the IDE Hook—zero code leaves the workstation. |
| **Offline & Air-Gapped Automation** | Engineers in restricted environments, on airplanes, or in field operations can automate file reorganization, data parsing, and scripting without internet connectivity. |
| **Cost Optimization for High-Volume Agents** | Repetitive agent actions (summaries, text transformations, draft replies, file searches) consume massive API credits. OmniAgent routes ~80% of routine steps to free local SLMs, slashing cloud API spend. |
| **Battery & Latency-Sensitive Edge Tasks** | Instead of incurring cloud round-trip latencies (800ms - 2500ms), local SLM execution on CPU/Vulkan returns in milliseconds for simple queries and notifications. |
| **IDE Copilot Enhancement via MCP** | Plug OmniAgent into Cursor or VS Code as a local MCP server to provide context-aware security auditing, instant code analysis, and smart task dispatching. |

---

## 🏛️ Architecture Overview

```
                          [ 🧠 User Prompt / IDE Hook / Dashboard ]
                                             |
                                     [ Task Router ]
                             (Complexity Scoring: 0.0 - 1.0)
                                    /               \
                       Score < Threshold         Score >= Threshold
                                  /                   \
                  [ 💻 Local SLM Engine ]       [ ☁️ Cloud LLM Adapter ]
                    Phi-4-mini / Qwen 2.5          OpenAI / Gemini
                    (CPU / Vulkan Native)         (Complex Reasoning)
                                  \                   /
                                   \                 /
                                  [ 🛠️ Tool Registry ]
                            (file_ops, code_runner, search)
                                             |
                                 [ Real-Time Event Bus ]
                              (SSE Stream -> Dashboard UI)
```

---

## 📁 Project Layout

```
OmniAgent/
├── agent/                       # Python Agent Orchestration Layer
│   ├── omniagent/
│   │   ├── __init__.py
│   │   ├── __main__.py          # CLI & REPL entrypoint
│   │   ├── config.py            # Pydantic configuration & env loader
│   │   ├── events.py            # Event bus (AgentEvent, EventType)
│   │   ├── executor.py          # Plan-route-execute loop
│   │   ├── ide_hook.py          # IDE Hook & MCP Server (JSON-RPC)
│   │   ├── memory.py            # Token-bounded conversation memory
│   │   ├── planner.py           # Multi-step goal decomposition
│   │   ├── router.py            # Hybrid complexity scoring & heuristics
│   │   ├── providers/           # Model providers (local, openai, gemini)
│   │   └── tools/               # Tool registry (file_ops, code_runner, etc.)
│   └── pyproject.toml
├── models/                      # Local GGUF Model Storage & Scripts
│   ├── download_model.py        # Model downloader with presets & progress
│   └── phi-4-mini.gguf          # Target local model file (downloaded)
├── dashboard/                   # Next.js 14 Real-Time Web Dashboard
│   ├── app/                     # App router, pages, and SSE API endpoints
│   ├── package.json
│   └── tailwind.config.ts
├── core/                        # Native C/C++ Inference Engine (Phase 2)
│   ├── CMakeLists.txt
│   ├── include/omni_engine.h    # C ABI interface
│   └── src/                     # Memory pool, engine, and tokenizer
├── .env.example                 # Example configuration template
└── README.md
```

---

## ⚙️ Prerequisites

- **Python**: `3.10` or higher
- **Node.js**: `18.0` or higher (for the web dashboard)
- **C++ Compiler** *(Optional)*: `GCC 9+` or `Clang 11+` with CMake `3.18+` (for compiling native C++ core)
- **RAM**: Minimum 8 GB (16 GB recommended for 7B models)

---

## 🚀 Quick Start

### 1. Clone & Set Up the Python Agent

```bash
git clone https://github.com/kingjethro999/OmniAgent.git
cd OmniAgent

# Create and activate a virtual environment
python3 -m venv agent/.venv
source agent/.venv/bin/activate

# Install agent package in editable mode
pip install -e agent
```

### 2. Configure Environment

Copy the example environment file and customize your settings:

```bash
cp .env.example .env
```

*(Optional: Add your `OPENAI_API_KEY` or `GEMINI_API_KEY` in `.env` if you wish to use cloud offloading for complex reasoning).*

---

## 📦 Model Setup & Management

OmniAgent utilizes GGUF Small Language Models for ultra-fast, privacy-preserving local execution. The built-in downloader script (`models/download_model.py`) includes curated presets optimized for local edge performance.

### Download Default Model (Microsoft Phi-4-mini 3.8B)

The default model is **Microsoft Phi-4-mini-Instruct (3.8B, Q4_K_M ~2.4 GB)**, which matches the engine's default `./models/phi-4-mini.gguf` target:

```bash
python3 models/download_model.py
```

### Download Alternative Presets

Depending on your hardware and bandwidth, you can download other models using the `--model` flag:

```bash
# High-speed 3B model (Qwen2.5-3B-Instruct, ~2.0 GB)
python3 models/download_model.py --model qwen-3b

# Maximum reasoning capacity (Qwen2.5-7B-Instruct, ~4.5 GB)
python3 models/download_model.py --model qwen-7b

# Meta Llama 3.2 (Llama-3.2-3B-Instruct, ~2.0 GB)
python3 models/download_model.py --model llama-3.2-3b

# View all available presets and sizes
python3 models/download_model.py --list
```

### Using Your Own GGUF Model

You can use any GGUF model from Hugging Face, LM Studio, or Ollama:
1. Copy your `.gguf` file to `./models/phi-4-mini.gguf`, **OR**
2. Point the environment variable in your `.env`:
   ```bash
   OMNI_LOCAL_MODEL_PATH="/path/to/your/custom-model.gguf"
   ```

---

## 💻 Using OmniAgent

### 1. CLI Quick Execution

Pass a goal directly as a command-line argument to run a single task:

```bash
# Routine task (routes automatically to Local SLM)
python -m omniagent "List all Python files in the current folder and count total lines"

# Complex task (routes automatically to Cloud LLM if keys configured)
python -m omniagent "Analyze the architectural trade-offs between monolithic and microservice architectures for edge computing"
```

### 2. Interactive Terminal REPL

Launch an interactive conversation session with color-coded live event telemetry:

```bash
python -m omniagent
```

Example interaction:
```
╭──────────────────────────────────────────────────╮
│ OmniAgent Engine v0.1.0                          │
│ Hybrid Local/Cloud Edge Agent Framework          │
╰──────────────────────────────────────────────────╯

OmniAgent: summarize the security features of this project
[THINKING] Received goal: summarize the security features of this project...
[ROUTING] Routed to local (score: 0.120)
[PLANNING] Created 1-step plan
[EXECUTING] Step 1/1: Summarize security features
[COMPLETED] Task completed successfully. (latency: 18.2ms)

╭─ Output ─────────────────────────────────────────╮
│ [Local SLM] Here is a concise summary of the     │
│ provided content. All data processed on-device.  │
╰──────────────────────────────────────────────────╯
```

Type `exit`, `quit`, or press `Ctrl+C` to leave the REPL.

---

### 3. Python SDK

Integrate OmniAgent directly into your Python scripts or Jupyter notebooks:

```python
import asyncio
from omniagent import AgentExecutor, TaskRouter, AgentMemory

async def main():
    # 1. Test routing classification
    router = TaskRouter()
    routing_result = router.route("Audit this cryptographic implementation for timing attacks")
    print(f"Decision: {routing_result.decision.value} (Score: {routing_result.complexity_score})")
    print(f"Reasoning: {routing_result.reasoning}")

    # 2. Run an end-to-end task
    executor = AgentExecutor()
    response = await executor.run("Organize my downloads folder and remove duplicate files")
    print(response)

if __name__ == "__main__":
    asyncio.run(main())
```

---

### 4. Real-Time Web Dashboard

OmniAgent includes a modern dashboard for visualizing agent execution in real-time.

```bash
cd dashboard
npm install
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) in your browser.

**Dashboard Capabilities**:
- **Live Reasoning Stream**: Watch agent events stream via SSE (`THINKING` ➔ `ROUTING` ➔ `PLANNING` ➔ `EXECUTING` ➔ `COMPLETED`).
- **Interactive Workbench**: Submit prompts, switch between on-device and cloud models, and view step-by-step tool results.
- **Cost & Latency Analytics**: Live tracking of local vs. cloud task execution ratio, milliseconds saved, and estimated API dollars saved.
- **Architecture Visualizer**: Interactive diagram showing system status across Python, C++ Core, and runtime layers.

---

## 🔌 IDE Hook & Model Context Protocol (MCP) Server

OmniAgent includes a built-in **IDE Hook and Model Context Protocol (MCP) server** (`omniagent.ide_hook`). It exposes a local HTTP JSON-RPC 2.0 interface that allows developer tools and AI IDEs to leverage OmniAgent for:
- **Local Code Audits**: Deep security and optimization analysis performed 100% on-device.
- **Smart Task Routing**: Classification queries for whether an inline prompt should run locally or offload to cloud.
- **MCP Tool Invocation**: Standardized access to OmniAgent's local tools.

### Starting the IDE Hook

You can launch the IDE Hook as a standalone service from the CLI:

```bash
# Default port: 8765
python -m omniagent --ide-hook

# Custom port
python -m omniagent --ide-hook --port 9000
```

You should see:
```
🚀 [IDE Hook / MCP] OmniAgent Bridge listening at http://127.0.0.1:8765
   • Endpoints: POST / (JSON-RPC 2.0: 'audit', 'route'), GET /status
   • Ready for Cursor, VS Code, JetBrains, and Claude/Antigravity hooks.
   • Press Ctrl+C to stop.
```

### Connecting to Cursor / VS Code / Claude

#### 1. Cursor IDE
Add OmniAgent to your project's `.cursor/mcp.json` or Cursor settings:

```json
{
  "mcpServers": {
    "omniagent": {
      "url": "http://127.0.0.1:8765",
      "type": "http"
    }
  }
}
```

#### 2. VS Code (Continue / Cline)
In your `config.json`:

```json
{
  "mcpServers": [
    {
      "name": "omniagent-local",
      "url": "http://127.0.0.1:8765"
    }
  ]
}
```

### API Endpoints & JSON-RPC Examples

#### Health & Status Check
```bash
curl -s http://127.0.0.1:8765/status | jq .
```
Response:
```json
{
  "status": "online",
  "engine": "OmniAgent v0.1.0",
  "ide_hook_version": "1.0",
  "mcp_protocol_version": "2024-11-05"
}
```

#### On-Device Code Audit (`audit`)
Audits code locally with privacy guarantees:
```bash
curl -X POST http://127.0.0.1:8765/ \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "method": "audit",
    "params": {
      "file_path": "auth.py",
      "code": "def login(user, pw): return db.query(f\"SELECT * FROM users WHERE u={user} AND p={pw}\")"
    },
    "id": 1
  }' | jq .
```
Response:
```json
{
  "jsonrpc": "2.0",
  "result": {
    "file_path": "auth.py",
    "analysis": "[Local SLM] Security Warning: Potential SQL Injection vulnerability detected...",
    "privacy_status": "PROCESSED_ON_DEVICE"
  },
  "id": 1
}
```

#### Routing Classification (`route`)
Queries the complexity router for decisions:
```bash
curl -X POST http://127.0.0.1:8765/ \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "method": "route",
    "params": {
      "prompt": "Find all occurrences of TODO in this directory"
    },
    "id": 2
  }' | jq .
```
Response:
```json
{
  "jsonrpc": "2.0",
  "result": {
    "decision": "local",
    "score": 0.125,
    "reasoning": "KW: 2L 0C | Len: 8w | Struct: 0 | Math: 0"
  },
  "id": 2
}
```

---

## 🛠️ Configuration Reference (.env)

All settings are managed via environment variables and loaded automatically at startup:

| Variable | Type | Default | Description |
|---|---|---|---|
| `OPENAI_API_KEY` | `string` | `""` | API key for OpenAI cloud routing. |
| `GEMINI_API_KEY` | `string` | `""` | API key for Google Gemini cloud routing. |
| `OMNI_CLOUD_PROVIDER` | `string` | `"openai"` | Preferred cloud provider (`"openai"` or `"gemini"`). |
| `OMNI_CLOUD_MODEL` | `string` | `"gpt-4o"` | Cloud model name for complex reasoning tasks. |
| `OMNI_LOCAL_MODEL_PATH` | `string` | `"./models/phi-4-mini.gguf"` | Absolute or relative path to the local GGUF model file. |
| `OMNI_COMPLEXITY_THRESHOLD` | `float` | `0.6` | Complexity threshold (0.0 - 1.0). Tasks scoring above this are offloaded to cloud. |
| `OMNI_LOCAL_MAX_TOKENS` | `int` | `2048` | Maximum context tokens for local SLM inference. |
| `OMNI_MAX_STEPS` | `int` | `10` | Maximum steps an agent can execute per task plan. |
| `OMNI_VERBOSE` | `bool` | `true` | Enables detailed logging of agent thoughts and tool actions. |
| `DASHBOARD_PORT` | `int` | `3000` | Port for the Next.js web dashboard. |
| `DASHBOARD_WS_PORT` | `int` | `3001` | Port for WebSocket / SSE stream events. |

---

## 🗺️ Roadmap

- [x] **Phase 1: Agent Core & Dashboard**
  - [x] Python plan-route-execute orchestration loop
  - [x] TaskRouter with multi-signal complexity heuristic scoring
  - [x] Built-in tool registry (`file_ops`, `code_runner`, `web_search`, `system_info`)
  - [x] GGUF model downloader with curated presets (`phi-4-mini`, `qwen-3b`, `qwen-7b`)
  - [x] IDE Hook & Model Context Protocol (MCP) server
  - [x] Real-time Next.js monitoring dashboard with SSE telemetry
- [x] **Phase 2: High-Performance C++ Inference Engine**
  - [x] Direct C ABI shared library bindings (`libomni_engine.so`)
  - [x] C++ JNI bridge (`libomni_engine_jni.so`) for Android / JVM runtimes
  - [x] Memory pool arena allocator and BPE tokenizer
- [x] **Phase 3: Multi-Platform Edge Runtimes**
  - [x] Enterprise Desktop worker (.NET 10 C#) with P/Invoke, document & code auditor, and dropzone folder watcher
  - [x] Consumer Mobile companion (Java / Android) with battery-aware NPU task routing and notification assistant

---

## 📄 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.
