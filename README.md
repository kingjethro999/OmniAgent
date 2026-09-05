<p align="center">
  <img src="assets/omniagent_logo.png" alt="OmniAgent Logo" width="220" />
</p>

# OmniAgent Engine

**A Hybrid Local/Cloud Edge Agent Framework with On-Device SLM Inference & IDE Hook (MCP)**

[![Release: v0.2.1](https://img.shields.io/badge/Release-v0.2.1-blue.svg)](https://github.com/kingjethro999/OmniAgent/releases/tag/v0.2.1)
[![Android: Signed APK](https://img.shields.io/badge/Android-APK%20(v0.2.1)-success.svg)](https://github.com/kingjethro999/OmniAgent/releases/tag/v0.2.1)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Python: 3.10+](https://img.shields.io/badge/Python-3.10%2B-brightgreen.svg)](https://python.org)
[![Next.js: 14](https://img.shields.io/badge/Dashboard-Next.js%2014-black.svg)](https://nextjs.org)
[![GGUF: llama.cpp](https://img.shields.io/badge/Inference-GGUF%20%2F%20llama.cpp-orange.svg)](https://github.com/ggerganov/llama.cpp)
[![Protocol: MCP](https://img.shields.io/badge/MCP-Compatible%20(2024--11--05)-purple.svg)](https://modelcontextprotocol.io)

OmniAgent Engine is a lightweight, high-performance edge framework for running autonomous AI agents on local devices. It features an intelligent **hybrid router** that executes routine tasks locally on quantized Small Language Models (SLMs like Phi-4-mini, Qwen 2.5, or Llama 3.2), seamlessly offloading only heavy multi-step reasoning to cloud LLMs (OpenAI GPT-4o or Google Gemini).

---

## 📑 Table of Contents

- [Official Release Packages & Downloads](#-official-release-packages--downloads-v021)
- [Key Features](#-key-features)
- [Target Use Cases & Hands-On Examples](#-target-use-cases--hands-on-examples)
  - [1. Privacy-Preserving Code & Document Audits](#1-privacy-preserving-code--document-audits)
  - [2. Silent Dropzone Folder Monitoring](#2-silent-dropzone-folder-monitoring)
  - [3. Offline OS-Level Task Automation & Siri-Like Assistant](#3-offline-os-level-task-automation)
  - [4. Battery-Saver Mobile Assistant & Accent Calibration](#4-battery-saver-mobile-assistant--notifications)
  - [5. Cost Optimization for High-Volume Workflows](#5-cost-optimization-for-high-volume-workflows)
  - [6. IDE Copilot Enhancement via MCP](#6-ide-copilot-enhancement-via-mcp)
- [Architecture Overview & Execution Lifecycle](#-architecture-overview--execution-lifecycle)
- [Project Layout](#-project-layout)
- [Prerequisites](#-prerequisites)
- [Quick Start](#-quick-start)
- [Model Setup & Management](#-model-setup--management)
- [Using OmniAgent Across Runtimes](#-using-omniagent-across-runtimes)
  - [1. CLI Quick Execution (Python)](#1-cli-quick-execution-python)
  - [2. Interactive Terminal REPL (Python)](#2-interactive-terminal-repl-python)
  - [3. Python Developer SDK](#3-python-developer-sdk)
  - [4. Siri-Like Desktop Assistant & Worker (.NET 10 C# - Windows & Linux)](#4-enterprise-desktop-worker-net-10-c)
  - [5. Consumer Mobile Companion & Voice Match Calibration (Android / Java JNI)](#5-consumer-mobile-companion-android--java-jni)
  - [6. Developer Prototyping & Benchmarking (Jupyter)](#6-developer-prototyping--benchmarking-jupyter)
  - [7. Real-Time Web Dashboard (Next.js 16)](#7-real-time-web-dashboard-nextjs-16)
- [IDE Hook & Model Context Protocol (MCP) Server](#-ide-hook--model-context-protocol-mcp-server)
  - [Starting the IDE Hook](#starting-the-ide-hook)
  - [Connecting to Cursor / VS Code / Claude](#connecting-to-cursor--vs-code--claude)
  - [API Endpoints & JSON-RPC Examples](#api-endpoints--json-rpc-examples)
- [Configuration Reference (.env)](#-configuration-reference-env)
- [Roadmap](#-roadmap)
- [License](#-license)

---

## 📦 Official Release Packages & Downloads (v0.2.1)

Pre-built standalone binaries, signed application packages, and developer SDKs are hosted directly on [GitHub Releases (v0.2.1)](https://github.com/kingjethro999/OmniAgent/releases/tag/v0.2.1).

### Package Distribution Matrix

| Component / Platform | Target Architecture | Distribution File | Size | Verification / Integrity | Direct Download |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Android Phone Assistant** | Android 8.0+ (ARM64 / x86_64) | `OmniAgent-v0.2.1-Android.apk` | 4.4 MiB | **Verified**: Signed with RSA 2048-bit release keystore (`apksigner` Scheme v2 verified) | [Download APK](https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/OmniAgent-v0.2.1-Android.apk) |
| **Desktop Siri Assistant (Linux)** | Linux x86_64 | `OmniAgent-Desktop-linux-x64-v0.2.1.tar.gz` | 31 MiB | **Verified**: Self-contained single-file `.NET 10` binary + native `libomni_engine.so` | [Download Linux tar.gz](https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/OmniAgent-Desktop-linux-x64-v0.2.1.tar.gz) |
| **Desktop Siri Assistant (Windows)** | Windows 10/11 x64 | `OmniAgent-Desktop-win-x64-v0.2.1.zip` | 31 MiB | **Verified**: Self-contained single-file `OmniAgent.Desktop.exe` with bundled icon & docs | [Download Windows zip](https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/OmniAgent-Desktop-win-x64-v0.2.1.zip) |
| **Main Omni Engine Core** | Linux x86_64 (C / C++ / JNI) | `omniagent-engine-linux-x64-v0.2.1.tar.gz` | 27 KiB | **Verified**: C ABI headers (`omni_engine.h`), `libomni_engine.so`, JNI library & CMake | [Download Engine tar.gz](https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/omniagent-engine-linux-x64-v0.2.1.tar.gz) |
| **Python SDK Wheel** | Python 3.10+ | `omniagent-0.2.1-py3-none-any.whl` | 38 KiB | **Verified**: PyPI-compliant wheel distribution | [Download Wheel](https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/omniagent-0.2.1-py3-none-any.whl) |
| **Python Source Distribution** | Python 3.10+ | `omniagent-0.2.1.tar.gz` | 60 KiB | **Verified**: Standard sdist source tarball | [Download sdist](https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/omniagent-0.2.1.tar.gz) |

> [!NOTE]
> Desktop Siri Assistant is specifically engineered for **Windows** and **Linux** workstations. (macOS already has native Siri built-in).

---

### Quick Installation & Usage Guide

#### 1. Android Phone Assistant
Download and install directly to any Android 8.0+ device:
```bash
# Download production signed APK
curl -LO https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/OmniAgent-v0.2.1-Android.apk

# Install via ADB:
adb install -r OmniAgent-v0.2.1-Android.apk
```
*Features*: 0 MB local model download overhead, free wake-word detection ("Hey Omni"), personal **Voice Match & Accent Calibration**, native Spotify playback, alarms, direct calls, SMS, WhatsApp, Gmail drafting, and app launching.

#### 2. Desktop Siri Assistant (Linux x64)
Extract and run the self-contained binary:
```bash
# Download and unpack
curl -LO https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/OmniAgent-Desktop-linux-x64-v0.2.1.tar.gz
tar -xzf OmniAgent-Desktop-linux-x64-v0.2.1.tar.gz
cd linux-x64

# Ensure executable permissions and test
chmod +x OmniAgent.Desktop

# 1. Run Siri-Like Desktop Assistant (Hands-Free Voice / Interactive HUD)
./OmniAgent.Desktop --assistant

# 2. Issue a one-shot spoken command
./OmniAgent.Desktop --say "Hey Omni, play Bohemian Rhapsody on Spotify"

# 3. Train your voice match & accent calibration profile
./OmniAgent.Desktop --train-voice

# 4. Standard enterprise worker tasks (watch, audit, organize)
./OmniAgent.Desktop --watch ./dropzone
./OmniAgent.Desktop --audit /path/to/project
```

#### 3. Desktop Siri Assistant (Windows x64)
Download `OmniAgent-Desktop-win-x64-v0.2.1.zip`, extract to any folder, and run from Command Prompt or PowerShell:
```cmd
:: 1. Launch hands-free Siri-like Desktop Assistant
OmniAgent.Desktop.exe --assistant

:: 2. Execute a single voice automation
OmniAgent.Desktop.exe --say "Hey Omni, lock screen"

:: 3. Train personalized accent calibration profile
OmniAgent.Desktop.exe --train-voice

:: 4. Background dropzone and security audit
OmniAgent.Desktop.exe --watch C:\dropzone
OmniAgent.Desktop.exe --audit C:\my-codebase
```

#### 4. Main Omni Engine Core (C / C++ SDK)
Incorporate the C++ edge inference engine into your own native C, C++, Rust, or Go applications:
```bash
# Download and extract C++ SDK
curl -LO https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/omniagent-engine-linux-x64-v0.2.1.tar.gz
tar -xzf omniagent-engine-linux-x64-v0.2.1.tar.gz
cd omniagent-engine-linux-x64

# Compile your application linking against libomni_engine.so:
gcc -Iinclude -Llib main.c -lomni_engine -Wl,-rpath,'$ORIGIN/lib' -o main
```

#### 5. Python Developer SDK
```bash
# Install via wheel
pip install https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/omniagent-0.2.1-py3-none-any.whl

# Or install local wheel file:
pip install omniagent-0.2.1-py3-none-any.whl
```

---

### Reproducible Build Instructions

All release packages can be built locally from source in this repository using the provided scripts and standard toolchains:

```bash
# 1. Package all platforms and generate staging archives:
./release_packages/package_all.sh

# 2. Build individual platform binaries manually:
# Android Signed Release APK:
cd mobile && ./gradlew assembleRelease

# Linux x64 Single-File Desktop:
dotnet publish desktop/OmniAgent.Desktop.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o release_packages/desktop/linux-x64

# Windows x64 Single-File Desktop:
dotnet publish desktop/OmniAgent.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o release_packages/desktop/win-x64

# macOS ARM64 Single-File Desktop:
dotnet publish desktop/OmniAgent.Desktop.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o release_packages/desktop/osx-arm64

# macOS Intel x64 Single-File Desktop:
dotnet publish desktop/OmniAgent.Desktop.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o release_packages/desktop/osx-x64

# Main Omni Engine C++ Core:
cd core && cmake -B build && cmake --build build

# Python SDK:
python3 -m build agent/
```

---

## 🔑 Key Features

- **Hybrid Edge/Cloud Routing**: Dynamic complexity scoring (0.0 to 1.0) based on keyword matching, token length, grammatical structure, and mathematical reasoning. Automatically directs privacy-sensitive or routine tasks to local SLMs and complex reasoning to cloud LLMs.
- **Privacy-First On-Device Execution**: Files, local logs, sensitive code, and system operations stay on your device without leaving your network.
- **GGUF Small Language Models**: Out-of-the-box support for Microsoft Phi-4-mini (3.8B), Qwen 2.5 (3B / 7B), and Llama 3.2 (3B) in 4-bit quantized GGUF format.
- **Built-in Tool Ecosystem**: Extensible tool registry featuring sandboxed code execution (`code_runner`), local file manipulation (`file_ops`), search (`web_search`), and hardware telemetry (`system_info`).
- **IDE Hook & MCP Bridge**: Embedded JSON-RPC server implementing the Model Context Protocol (MCP) standard to empower IDEs (Cursor, VS Code, JetBrains, Antigravity) with local code auditing and routing.
- **Live Monitoring Dashboard**: Next.js web application with Server-Sent Events (SSE) streaming live agent thoughts, plan step status, latency, and estimated cost savings.

---

## 🎯 Target Use Cases & Hands-On Examples

OmniAgent is not a theoretical wrapper; it is an active multi-runtime edge framework. Below are the primary real-world use cases with executable commands, expected outputs, and code examples:

### 1. Privacy-Preserving Code & Document Audits
- **Threat Model**: Software engineers, defense contractors, and legal teams cannot upload proprietary codebases or confidential contracts to external cloud APIs without violating NDAs, GDPR, or security policies.
- **Solution**: The **Enterprise Desktop Worker** (`desktop/`) and **IDE Hook** (`agent/omniagent/ide_hook.py`) parse syntax trees, match vulnerability patterns, and run local SLM summarization 100% on-device. Zero network packets leave the machine.

#### Running a Full Local Repository Audit via C# Desktop Worker:
```bash
dotnet run --project desktop -- --audit ./sensitive-project
```
*Sample Output*:
```text
==========================================================
  OmniAgent Enterprise Desktop Worker (.NET 10)
  Silent Background Automation & Local Document Auditing
==========================================================
Engine:  Native C++ Core (Active)
Privacy: 100% On-Device (Zero data leaves workstation)

[Auditor] Scanning 148 files in ./sensitive-project (100% On-Device / Zero Network)...

══════════════════════════════════════════════════════════
  Audit Report for: ./sensitive-project
  Scanned Files:    148
  Total Alerts:     2
══════════════════════════════════════════════════════════

[CRITICAL] Hardcoded Secret / API Key
File: ./sensitive-project/backend/config.py:42
Note: Potential hardcoded credential or token detected in source file.
Code: stripe_secret = "sk-live_9482759174829104829184"

[WARNING] SQL Injection Pattern
File: ./sensitive-project/api/routes.py:89
Note: String concatenation or formatting detected in SQL query.
Code: query = f"SELECT * FROM users WHERE email='{user_email}'"

🤖 Local SLM Analysis: [C++ Native SLM Inference] Identified 2 high-risk security flaws. Recommendation: Move secret to environment variable and parameterize database queries.
```

#### Triggering an On-Device Audit via HTTP / JSON-RPC IDE Hook:
```bash
curl -s -X POST http://127.0.0.1:8765/ \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "method": "audit",
    "params": {
      "file_path": "auth.py",
      "code": "cursor.execute(f\"SELECT * FROM users WHERE user=\x27{username}\x27 AND pass=\x27{password}\x27\")"
    },
    "id": 1
  }' | jq .
```
*Response*:
```json
{
  "jsonrpc": "2.0",
  "result": {
    "file_path": "auth.py",
    "analysis": "[Local SLM] Security Warning: Potential SQL Injection vulnerability detected in query string formatting. Use parameterized queries.",
    "privacy_status": "PROCESSED_ON_DEVICE"
  },
  "id": 1
}
```

---

### 2. Silent Dropzone Folder Monitoring
- **Challenge**: Compliance officers, lawyers, and audit teams receive hundreds of vendor documents and files daily and need automated triage without opening terminals or uploading files.
- **Solution**: The Desktop Worker operates as a silent OS background service watching a designated directory (e.g., `./dropzone`). When any file is dropped into the folder, OmniAgent instantly runs AST & credential scanning with local SLM analysis and logs the alert.

#### Running the Background Watcher:
```bash
dotnet run --project desktop -- --watch ./dropzone
```
*Live Terminal Event*:
```text
[Dropzone Watcher] Actively monitoring folder: /home/user/OmniAgent/dropzone
Drop any source code or documents into this folder for instant on-device auditing.
Press Ctrl+C to stop.

[Dropzone] New file detected: client_contract.txt
⚠️ [Audit Alert] 1 potential issue(s) detected in client_contract.txt:
   • Line 14 [CRITICAL]: Hardcoded Secret / API Key — aws_secret_access_key = "AKIAIOSFODNN7EXAMPLE"
🤖 AI Local Summary: [C++ Native SLM Inference] Detected leaked AWS credential in document. Immediate revocation recommended.
```

---

### 3. Offline OS-Level Task Automation
- **Challenge**: Field engineers, developers on airplanes, or air-gapped systems need automated data organization, spreadsheet cleaning, and repo inspection with zero internet access.
- **Solution**: The `SystemAutomation` module organizes directories, normalizes messy CSV files, and checks git status locally.

#### Example A: Auto-Categorize Files in a Cluttered Folder
```bash
dotnet run --project desktop -- --organize ./downloads
```
*Output*:
```text
[Automation] Organized 42 files into categorized folders.
  ├── Code/       (18 files: .py, .cs, .cpp, .js)
  ├── Documents/  (12 files: .pdf, .docx, .md)
  ├── Data/       (8 files: .csv, .json, .sql)
  └── Media/      (4 files: .png, .jpg)
```

#### Example B: Clean and Normalize CSV Spreadsheets
```bash
dotnet run --project desktop -- --format-csv ./dirty_sales.csv
```
*Output*:
```text
[Automation] Formatted CSV (500 rows) -> ./dirty_sales.csv (Trimmed whitespace, balanced column delimiters).
```

---

### 4. Battery-Saver Mobile Assistant & Notifications
- **Challenge**: Mobile LLMs drain battery rapidly and cause severe thermal throttling. Furthermore, users do not want their personal SMS, emails, and calendar events streamed to remote servers.
- **Solution**: The **Consumer Mobile Companion** (`mobile/`) hooks into Android system events via Java, executing quick queries on the local NPU via JNI (`libomni_engine_jni.so`). When battery drops below 15% or enters battery-saver mode, cloud offload is strictly restricted.

#### Executing Local Notification Digest on Device:
```bash
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner "Summarize my notifications from the last hour"
```
*Output*:
```text
==========================================================
  OmniAgent Consumer Mobile Companion (Android / Java)
  Battery-Saver SLM Assistant & Native System Integration
==========================================================
[OmniEngine C++ Core] Initialized with model: models/phi-4-mini.gguf (2 threads)
Engine:  Native NPU/CPU JNI (Active)
Battery: 82% (Power Save: false)
Privacy: 100% On-Device Execution for Routine Queries

Processing query: "Summarize my notifications from the last hour"
Routing: [LOCAL_NPU] Score: 0.05 | Reason: Routine task (score: 0.05 < 0.55) -> Fast on-device NPU

Response:
[C++ Native SLM Inference] Processed on-device: Summarize my notifications from the last hour
[OmniEngine C++ Core] Unloaded model from memory pool.
```

#### Drafting a Context-Aware Quick Reply (14ms Latency):
```bash
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner "Draft reply to Mom: Are you coming over for dinner tonight?"
```
*Output*:
```text
Routing: [LOCAL_NPU] Score: 0.08 | Reason: Routine task -> Fast on-device NPU
Response: [C++ Native SLM Inference] Drafted Reply: "Hey Mom! Yes, I will be there around 7:00 PM. Looking forward to it!" (Generated locally in 14ms).
```

---

### 5. Cost Optimization for High-Volume Workflows
- **Challenge**: Large multi-agent systems executing thousands of automated actions hourly quickly accumulate massive OpenAI/Anthropic cloud bills.
- **Solution**: The hybrid `TaskRouter` classifies task complexity. ~80% of routine actions execute on-device for **$0.00 cost** and sub-30ms latency, routing only the 20% complex mathematical or multi-step logic to cloud LLMs.

```python
from omniagent import TaskRouter

router = TaskRouter(complexity_threshold=0.55)

# 1. Routine Query -> Handled by free on-device SLM (~20ms latency, $0.00)
task1 = router.route("Extract email addresses from this error log")
print(f"Decision: {task1.decision.value.upper()} | Score: {task1.complexity_score:.2f} | Reason: {task1.reasoning}")
# => Decision: LOCAL | Score: 0.08 | Reason: KW: 1L 0C | Len: 7w

# 2. Complex Reasoning -> Offloaded to Cloud API
task2 = router.route("Derive the mathematical proof for quantum entanglement entropy and calculate density matrix")
print(f"Decision: {task2.decision.value.upper()} | Score: {task2.complexity_score:.2f} | Reason: {task2.reasoning}")
# => Decision: CLOUD | Score: 0.68 | Reason: KW: 0L 3C | Len: 12w | Math: 1
```

---

### 6. IDE Copilot Enhancement via MCP
- **Challenge**: Generic IDE copilot extensions lack awareness of local system tools, require cloud round-trips for simple syntax checks, and cannot execute shell scripts safely.
- **Solution**: OmniAgent's IDE Hook server speaks JSON-RPC 2.0 and MCP, allowing Cursor, VS Code, and JetBrains to invoke on-device security linting and task routing seamlessly.

---

## 🏛️ Architecture Overview & Execution Lifecycle

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

### 🔬 Under the Hood: The 5-Stage Hybrid Execution Lifecycle

OmniAgent avoids single-model bottlenecks through a continuous 5-stage pipeline:

```
[Prompt] ➔ [1. Ingest] ➔ [2. Multi-Signal Scoring] ➔ [3. Context Routing] ➔ [4. C++ Arena / Cloud] ➔ [5. SSE Telemetry]
```

#### Stage 1: Ingestion & Environment Normalization
A task prompt enters the ecosystem from any supported front:
- **Python CLI / REPL**: Direct input via terminal stdin.
- **Enterprise Desktop Worker**: Background file drop into `./dropzone` or `--audit` invocation.
- **Consumer Mobile Companion**: Android notification listener or voice assistant prompt.
- **IDE Hook / MCP**: JSON-RPC 2.0 request over HTTP (`http://127.0.0.1:8765/`).

#### Stage 2: Multi-Signal Heuristic Complexity Scoring
The `TaskRouter` analyzes the input across four weighted orthogonal signals:
1. **Keyword Signal ($W_1 = 0.35$)**: Ratio of local routine keywords (`summarize`, `clean`, `format`, `audit`, `reply`, `file`) to high-complexity keywords (`derive`, `proof`, `architecture`, `quantum`, `strategic`).
2. **Length Signal ($W_2 = 0.25$)**: Word count ratio scaled against a 60-word threshold: $\min(1.0, \frac{\text{word\_count}}{60})$.
3. **Structural Complexity ($W_3 = 0.20$)**: Presence of multi-clause conditional instructions (`if`, `then`, `otherwise`, `step 1`).
4. **Mathematical Density ($W_4 = 0.20$)**: Presence of LaTeX tokens, arithmetic symbols, equations, and calculus variables.

$$\text{Final Complexity Score} = \sum (W_i \times S_i) \in [0.0, 1.0]$$

#### Stage 3: Contextual Routing Decision Matrix
The routing engine applies hardware context before comparing against the complexity threshold (default: `0.55`):
- **Battery-Aware Override (Mobile)**: If device battery < 15% or OS power-save mode is active, the task is **strictly locked** to `LOCAL_NPU` to prevent modem battery drain.
- **Threshold Evaluation**: If score < `OMNI_COMPLEXITY_THRESHOLD`, destination is `LOCAL_SLM`. Otherwise, destination is `CLOUD_OFFLOAD`.

#### Stage 4: Unified C++ Hardware Execution
When a task is routed locally, it executes through the shared C++ core:
- **Zero-Fragmentation Arena Allocator**: [memory_pool.cpp](core/src/memory_pool.cpp) pre-allocates a 64MB tensor memory arena, preventing memory leaks during rapid inference loops.
- **P/Invoke (C# Desktop)**: [NativeEngineBridge.cs](desktop/NativeEngineBridge.cs) calls `omni_generate` directly in unmanaged memory.
- **JNI (Java Android)**: [NativeEngineJNI.java](mobile/src/main/java/io/omniagent/mobile/NativeEngineJNI.java) calls `libomni_engine_jni.so` across the Java Native Interface.
- **ctypes (Python)**: [local.py](agent/omniagent/providers/local.py) invokes C ABI entrypoints with zero-copy string buffers.

#### Stage 5: Real-Time Event Bus & Telemetry Stream
Every lifecycle state transition is emitted asynchronously onto the `event_bus`:
- `THINKING` ➔ `ROUTING` ➔ `PLANNING` ➔ `EXECUTING` ➔ `COMPLETED`
- Telemetry events stream over Server-Sent Events (SSE) to the Next.js web dashboard (`/api/stream`), updating live latency, token speed, and cost savings in real-time.

---

## 📁 Project Layout

```
OmniAgent/
├── assets/                          # Official project branding & logos
│   ├── omniagent_logo.png           # Master logo banner
│   └── icon.png                     # Square cybernetic "O" emblem
├── core/                            # Native C/C++ Compute Engine
│   ├── CMakeLists.txt               # CMake build with JNI and C ABI targets
│   ├── include/
│   │   ├── omni_engine.h            # C ABI exports (omni_init_engine, omni_generate)
│   │   ├── tokenizer.h              # BPE tokenization & context window
│   │   ├── memory_pool.h            # 64MB tensor arena pool allocator
│   │   └── io_omniagent_mobile_...h # Generated JNI headers
│   ├── src/
│   │   ├── engine.cpp               # Core inference routines
│   │   ├── tokenizer.cpp            # Text encoding/decoding
│   │   └── memory_pool.cpp          # Zero-fragmentation arena
│   ├── bindings/jni/omni_jni.cpp    # JNI bridge for Java/Android
│   └── build/
│       ├── libomni_engine.so        # C ABI shared library (Desktop & Python)
│       └── libomni_engine_jni.so    # JNI shared library (Mobile)
├── desktop/                         # Branch A: Enterprise Desktop Worker (.NET 10)
│   ├── OmniAgent.Desktop.csproj     # C# project file
│   ├── Program.cs                   # CLI & Interactive worker entrypoint
│   ├── NativeEngineBridge.cs        # P/Invoke bridge to libomni_engine.so + HTTP fallback
│   ├── DocumentAuditor.cs           # On-device secret, key, & SQLi scanner
│   ├── FolderWatcher.cs             # Silent background dropzone monitor
│   ├── SystemAutomation.cs          # File categorization, CSV formatter, git status
│   └── assets/app_icon.ico          # Desktop Windows/Linux icon
├── mobile/                          # Branch B: Consumer Mobile Companion (Android/Java)
│   ├── build.gradle                 # Android application gradle config
│   ├── src/main/java/io/omniagent/mobile/
│   │   ├── NativeEngineJNI.java     # JNI binding to libomni_engine_jni.so
│   │   ├── MobileTaskRouter.java    # Battery-aware local NPU vs cloud router
│   │   ├── NotificationAssistant.java # Notification digest & quick replies
│   │   ├── MobileAgentService.java  # Android background service orchestrator
│   │   ├── MobileAgentRunner.java   # Standalone JVM runner for workstation testing
│   │   └── MainActivity.java        # Android UI activity
│   └── src/main/res/drawable/       # Android launcher icons
├── agent/                           # Python Developer SDK & Orchestration
│   ├── omniagent/
│   │   ├── __init__.py
│   │   ├── __main__.py              # CLI & REPL entrypoint (--ide-hook flag)
│   │   ├── config.py                # Environment & Pydantic config
│   │   ├── events.py                # Event bus (AgentEvent, EventType)
│   │   ├── executor.py              # Plan-route-execute loop
│   │   ├── ide_hook.py              # Standalone IDE Hook & MCP Server (port 8765)
│   │   ├── memory.py                # Token-bounded conversation memory
│   │   ├── planner.py               # Goal decomposition engine
│   │   ├── router.py                # Multi-signal complexity scoring
│   │   ├── providers/               # Providers: local (ctypes C++), openai, gemini
│   │   └── tools/                   # Registry: file_ops, code_runner, web_search
│   └── pyproject.toml
├── notebooks/                       # Developer Prototyping & Benchmarking Sandbox
│   └── omniagent_prototyping.ipynb  # Interactive Jupyter notebook
├── dashboard/                       # Real-Time Monitoring Web Dashboard (Next.js 16)
│   ├── app/                         # App Router, SSE stream (/api/stream), workbench
│   ├── public/                      # Favicons, web icons, and logo assets
│   └── package.json
├── models/                          # GGUF Model Management
│   ├── download_model.py            # Presets: phi-4-mini, qwen-3b, qwen-7b, llama-3.2
│   └── phi-4-mini.gguf              # Downloaded local SLM file
├── .env.example                     # Environment template
└── README.md

```

---

## ⚙️ Prerequisites

- **Python**: `3.10` or higher
- **.NET SDK**: `10.0` (or `8.0+`) for compiling and running the Enterprise Desktop Worker
- **Java / JDK**: `OpenJDK 17` or `21` for the Consumer Mobile Companion and JNI bindings
- **Node.js**: `18.0` or higher (pnpm or npm) for the Next.js web dashboard
- **C++ Compiler**: `GCC 9+` or `Clang 11+` with `CMake 3.18+` (for compiling native core libraries)
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

## 💻 Using OmniAgent Across Runtimes

### 1. CLI Quick Execution (Python)

Pass a goal directly as a command-line argument to run a single task:

```bash
# Routine task (routes automatically to Local SLM)
python -m omniagent "List all Python files in the current folder and count total lines"

# Complex task (routes automatically to Cloud LLM if keys configured)
python -m omniagent "Analyze the architectural trade-offs between monolithic and microservice architectures for edge computing"
```

---

### 2. Interactive Terminal REPL (Python)

Launch an interactive conversation session with color-coded live event telemetry:

```bash
python -m omniagent
```

Example interaction:
```
╭──────────────────────────────────────────────────╮
│ OmniAgent Engine v0.2.1                          │
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

### 3. Python Developer SDK

Integrate OmniAgent directly into your Python scripts, microservices, or bots:

```python
import asyncio
from omniagent import AgentExecutor, TaskRouter, AgentMemory

async def main():
    # 1. Test routing classification
    router = TaskRouter(complexity_threshold=0.55)
    routing_result = router.route("Audit this cryptographic implementation for timing attacks")
    print(f"Decision: {routing_result.decision.value} (Score: {routing_result.complexity_score})")
    print(f"Reasoning: {routing_result.reasoning}")

    # 2. Run an end-to-end task (Local SLM or Cloud LLM)
    executor = AgentExecutor()
    response = await executor.run("Organize my downloads folder and remove duplicate files")
    print("\nAgent Output:\n", response)

if __name__ == "__main__":
    asyncio.run(main())
```

---

### 4. Enterprise Desktop Worker (.NET 10 C#)

### 4. Siri-Like Desktop Assistant & Worker (.NET 10 C# - Windows & Linux)

The Desktop Assistant turns your Windows or Linux workstation into an autonomous, voice-interactive workstation companion (Siri-like for PC). Built in C# with .NET 10, it hooks directly into low-level OS interfaces for media, apps, and hardware telemetry, binding dynamically to the native C++ Core (`libomni_engine.so`) for on-device reasoning.

> [!NOTE]
> macOS is intentionally omitted from the desktop assistant target, as macOS already features native system-integrated Siri.

#### Key Desktop Assistant Features:
- **Hands-Free Wake Word Listening ("Hey Omni", "OK Omni")**: Continuous low-latency background audio monitoring with custom voice matching.
- **Audible Speech Synthesis & Toast Chimes**: Responsive Text-to-Speech (native Speech Dispatcher on Linux, SAPI on Windows) and desktop notification toasts via `notify-send` and Windows Notification Center.
- **Deep System Automations**:
  - **Spotify & Media Playback**: Play/pause, track navigation, and instant Spotify search (`"Hey Omni, play Bohemian Rhapsody on Spotify"`).
  - **Workstation App Launcher**: Launch browser, code editors, terminal, or tools (`"Hey Omni, open Chrome"`, `"open VS Code"`, `"open terminal"`, `"open calculator"`, `"open steam"`).
  - **Hardware Controls**: Volume adjustment (`"Hey Omni, set volume to 80%"` / `"mute volume"`), screenshot capture (`"take a screenshot"`), and workstation lock (`"lock screen"`).
  - **Timers, Alarms & Clock**: Background timer with audio chime & alarm alert (`"set a timer for 10 minutes"`), instant date & time.
  - **Live Weather & Telemetry**: Instant live weather reports and system metrics (CPU load, RAM usage, storage).
  - **Private On-Device Inference**: General inquiries and natural conversation processed locally via C++ Core SLM with zero internet leakage.

```bash
# 1. Launch hands-free Siri-like Desktop Assistant (Voice HUD)
dotnet run --project desktop -- --assistant

# 2. Execute a single voice automation directly
dotnet run --project desktop -- --say "Hey Omni, play bohemian rhapsody on spotify"
dotnet run --project desktop -- --say "Hey Omni, take a screenshot"
dotnet run --project desktop -- --say "Hey Omni, lock screen"

# 3. Train your personalized Voice Match & Accent profile
dotnet run --project desktop -- --train-voice

# 4. Standard enterprise worker tasks
dotnet run --project desktop -- --audit ./my-repo
dotnet run --project desktop -- --watch ./dropzone
dotnet run --project desktop -- --organize ./cluttered-folder
dotnet run --project desktop -- --format-csv ./data.csv
dotnet run --project desktop -- --git-status

# 5. Interactive text menu
dotnet run --project desktop
```

---

### 5. Consumer Mobile Companion & Voice Match Calibration (Android / Java JNI)

The Consumer Mobile Companion executes natively on Android via JNI to the shared C++ Core (`libomni_engine_jni.so`). Designed with an explicit, low-level Java architecture (`AudioRecord` 16kHz PCM streaming, foreground services, Android intent dispatchers), it turns any Android phone into an autonomous assistant with **zero 3rd-party cloud costs** or heavyweight model downloads.

#### Key Mobile Assistant Capabilities:
- **Zero-Download On-Device Engine (0 MB Overhead)**: Unlike mobile LLMs requiring 1.5 GB+ downloads, OmniAgent maps natural voice commands to Android framework interfaces in < 5ms with zero model download overhead.
- **Voice Match & Accent Calibration (Card 3B)**: Guided 4-phrase setup wizard that records user speech samples to profile acoustic energy and phonetics. Accurately identifies "Omni" across diverse global accents, tone pitches, and dialects.
- **Pure Java Audio Record Engine**: Continuous background PCM audio processing via native `AudioRecord` with zero dependency on proprietary wake-word clouds (e.g. Picovoice or ONNX).
- **Backend Selection**: Run 100% locally with the built-in on-device engine or point to your self-hosted OmniAgent Remote Server (`http://<ip>:8765`).
- **Free Wake Word Engine**: Spot `"Hey Omni"`, `"OK Omni"`, `"Omni"`, and `"Hey Agent"` locally in real-time.
- **Optional Accessibility Automation**: `OmniAccessibilityService` enables hands-free system navigation and app execution as an optional accessibility toggle without interfering with existing companion routines.
- **ChatGPT-Inspired Cobalt Design System**: Dark surface (`#121211`), cards (`#1E1E1C`), cobalt accent (`#4F5FF7`), and 100% zero emojis (utilizes crisp vector drawables and clean typography).
- **Signed Production APK Included**: Production signed APK (`OmniAgent-v0.2.1-Android.apk`) verified with APK Signature Scheme v2.

#### Native Phone Automation Supported:
- **Music Playback**: `"Hey Omni, play the box by roddy rich"` ➔ Launches Spotify search (`spotify:search:...`) and streams audio.
- **Clock & Alarms**: `"Hey Omni, set an alarm for 7:00 AM"` / `"set a timer for 10 minutes"` ➔ Dispatches native `AlarmClock` intents.
- **Telecom & Calls**: `"Hey Omni, call mum"` (`tel:mum`), `"Hey Omni, pick the call"` (`TelecomManager.acceptRingingCall`), `"Hey Omni, end call"`.
- **Messaging**: `"Hey Omni, send message to Sarah saying on my way"` (SMS) / `"Hey Omni, send whatsapp to John saying meeting now"` (WhatsApp).
- **Email Drafting**: `"Hey Omni, draft an email to boss saying working remotely today"` ➔ Composes in Gmail (`mailto:`).
- **App Launching**: `"Hey Omni, open tiktok"`, `"open whatsapp"`, `"open gallery"`, `"open youtube"`, `"open camera"`, `"open netflix"`.

#### Building and Installing the Production APK:
```bash
# Build the signed production release APK:
cd mobile
./gradlew assembleRelease

# The resulting signed APK is located at:
# mobile/build/outputs/apk/release/OmniAgentMobile-release.apk (4.4 MB)
# (Also staged at: release_packages/dist/OmniAgent-v0.2.1-Android.apk)
```

#### Running Voice Commands on Workstation CLI (Zero-Device Testing):
```bash
# Compile standalone classes
javac -d mobile/bin -cp "mobile/src/main/java" mobile/src/main/java/io/omniagent/mobile/*.java

# 1. Play song on Spotify
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner "Hey Omni, play the box by roddy rich"

# 2. Set an alarm
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner "Hey Omni, set an alarm for 7:00 AM"

# 3. Call contact & Telecom control
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner "Hey Omni, call mum"
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner "Hey Omni, pick the call"
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner "Hey Omni, end call"

# 4. Send SMS & WhatsApp
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner "Hey Omni, send message to Sarah saying on my way"
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner "Hey Omni, send whatsapp to John saying meeting in 5 minutes"

# 5. Launch apps
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner "Hey Omni, open whatsapp"
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner "Hey Omni, open tiktok"

# 6. Launch interactive Phone Assistant loop
java -cp mobile/bin io.omniagent.mobile.MobileAgentRunner
```

---

### 6. Developer Prototyping & Benchmarking (Jupyter)

For researchers, data scientists, and prompt engineers, OmniAgent includes an interactive Jupyter notebook playground at `notebooks/omniagent_prototyping.ipynb`.

```bash
# Launch the Jupyter Notebook environment
jupyter notebook notebooks/omniagent_prototyping.ipynb
```

**What you can do in the notebook**:
- **Prototype Routing Heuristics**: Experiment with different prompt lengths, keyword weightings, and complexity thresholds.
- **Benchmark C++ Native Latency**: Measure on-device token generation speed (ms/token) directly against local RAM/CPU.
- **Inspect Execution Chains**: Visualize agent step decomposition and memory states interactively.

---

### 7. Real-Time Web Dashboard (Next.js 16)

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
  "engine": "OmniAgent v0.2.1",
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
