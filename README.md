# 🧠 OmniAgent Engine

**A Hybrid Local/Cloud Edge Agent Framework**

OmniAgent Engine is a lightweight, lightning-fast cross-platform framework for building and running autonomous AI agents that operate locally on edge devices but can intelligently offload heavy reasoning tasks to the cloud when needed.

## 🏛️ Architecture

```
                       [ 💾 The Core C++ Engine ]
                       (Inference, Local RAM, WebGPU)
                                   |
           ________________________|________________________

          |                                                 |
[ 💻 Enterprise Desktop ]                     [ 📱 Consumer Mobile ]
 C# (.NET / Avalonia)                          Java (Android Native)
 Privacy, System Automation                    Battery, Daily Assistant

          |                                                 |
[ 🌐 JS Control Panel ]                       [ 🐍 Python/Jupyter SDK ]
```

## 🚀 Quick Start

### 1. Agent Brain (Python)
```bash
cd agent
pip install -e .
python -m omniagent
```

### 2. Dashboard (JavaScript)
```bash
cd dashboard
npm install
npm run dev
```

## 📁 Project Structure

| Directory | Language | Purpose |
|-----------|----------|---------|
| `agent/` | Python | Agent orchestration, planning, routing |
| `core/` | C/C++ | Local inference engine (Phase 2) |
| `dashboard/` | JavaScript | Real-time monitoring UI |
| `desktop/` | C# | Enterprise desktop runtime (Phase 3) |
| `mobile/` | Java | Consumer mobile runtime (Phase 3) |

## 🔑 Key Features

- **Hybrid Routing** — Automatically routes tasks between local SLMs and cloud LLMs
- **Privacy-First** — Sensitive data stays on-device; only complex tasks hit the cloud
- **Battery-Efficient** — 80% of routine tasks run on tiny 1B-3B models locally
- **True Automation** — Agents don't just generate text, they *do things*
- **Plugin System** — Extensible tool architecture for custom capabilities
- **Real-Time Monitoring** — WebSocket-powered dashboard shows agent reasoning live

## 📄 License

MIT License — See [LICENSE](LICENSE) for details.
