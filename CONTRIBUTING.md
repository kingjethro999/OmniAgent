# 🤝 Contributing to OmniAgent Engine

Thank you for your interest in contributing to **OmniAgent Engine** — the hybrid local/cloud edge agent framework!

## 🏛️ Ecosystem Architecture

OmniAgent is organized into a modular multi-language structure:

- **`agent/` (Python 3.10+)**: Agent orchestration, task router, planning engine, tools, memory, and IDE hook bridge.
- **`core/` (C/C++17)**: High-performance inference engine, hardware tensor arena, BPE tokenizer, and C ABI dynamic exports (`omni_engine.h`).
- **`dashboard/` (Next.js 16 + React 19 + TypeScript + Tailwind v4)**: Real-time telemetry, interactive task simulator, app download hub, and reasoning feed.
- **`desktop/` (C# / .NET 10 + Photino.NET)**: Native Desktop Voice Assistant, floating voice orb HUD, WebKitGTK/WebView2 shell, and low-level system automation.
- **`mobile/` (Java 17 / Android Native SDK 34)**: Consumer Mobile companion app, voice match calibration, hands-free automation, and JNI NPU bridge.

---

## 🛠️ Development Setup

### 1. Python Agent Brain

```bash
cd agent
python3 -m venv .venv
source .venv/bin/activate
pip install -e .
python -m omniagent "Summarize local files"
```

### 2. Next.js Dashboard

```bash
cd dashboard
pnpm install
pnpm dev
```

### 3. C++ Core Compute Engine

```bash
cd core
mkdir -p build && cd build
cmake ..
make -j4
```

### 4. C# Desktop Voice Assistant GUI (.NET 10)

```bash
cd desktop
dotnet build
# Run native floating GUI assistant window:
dotnet run
# Or test CLI commands:
dotnet run -- --say "Check workstation status"
```

### 5. Android Mobile Companion (APK)

```bash
cd mobile
./gradlew assembleRelease
# Output: mobile/build/outputs/apk/release/OmniAgentMobile-release.apk
```

---

## 📐 Coding Conventions

- **Python**: Follow PEP 8 guidelines. Use type hints (`typing`) and Pydantic models for data structures.
- **C/C++**: C++17 standard. Export functions using `extern "C"` ABI declarations in `omni_engine.h`.
- **TypeScript / React**: Use strict functional components, Tailwind CSS variables, and Lucide icons.
- **Commit Messages**: Follow standard conventional commits format (e.g., `feat: add IDE hook MCP server`, `fix: resolve router score threshold`).

---

## 📄 License & Submissions

By submitting a Pull Request, you agree that your contributions will be licensed under the project's [MIT License](LICENSE).
