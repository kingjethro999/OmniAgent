# 🤝 Contributing to OmniAgent Engine

Thank you for your interest in contributing to **OmniAgent Engine** — the hybrid local/cloud edge agent framework!

## 🏛️ Ecosystem Architecture

OmniAgent is organized into a modular multi-language structure:

- **`agent/` (Python 3.10+)**: Agent orchestration, task router, planning engine, tools, memory, and IDE hook bridge.
- **`core/` (C/C++17)**: High-performance inference engine, hardware tensor arena, BPE tokenizer, and C ABI dynamic exports (`omni_engine.h`).
- **`dashboard/` (Next.js 16 + React 19 + TypeScript + Tailwind v4)**: Real-time telemetry, interactive task simulator, and reasoning feed.
- **`desktop/` (C# / .NET 8)**: Enterprise Desktop background service and P/Invoke bridge.
- **`mobile/` (Java / Android Native)**: Consumer Mobile companion app and JNI NPU bridge.

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
mkdir build && cd build
cmake ..
make -j4
```

### 4. C# Enterprise Desktop Shell
```bash
cd desktop
dotnet build
dotnet run
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
