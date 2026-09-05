## OmniAgent v0.2.0 — Consumer Phone Assistant, Cross-Platform Desktop & Production Packages

### What's New in v0.2.0:

- **Zero-Download On-Device Engine (0 MB Overhead)**: Bypasses heavy 1.5GB+ model downloads on Android by mapping natural language commands directly to Android framework capabilities and native intents in < 5ms.
- **Hands-Free Phone Voice Assistant**: Free on-device wake-word detection for `"Hey Omni"`, `"OK Omni"`, `"Omni"`, and `"Hey Agent"`.
- **Native Phone Automations**: Hands-free Spotify media playback, alarms, timers, direct phone calls, call answering/ending, SMS, WhatsApp, Gmail drafting, and app launching (TikTok, WhatsApp, Photos, YouTube, Netflix, Uber, and more).
- **Optional Accessibility Automation Service**: `OmniAccessibilityService` isolates assistant automation as an optional user accessibility setting without interfering with existing companion routines.
- **Clean Consumer UI**: Removed developer technical jargon from setup and activity monitors in favor of conversational dialogue (`You: ...` / `OmniAgent: ...`). Supports switching between Built-in On-Device Assistant and a self-hosted Remote Server.
- **ChatGPT-Inspired Cobalt Design System**: Dark canvas (`#121211`), cards (`#1E1E1C`), cobalt accent (`#4F5FF7`), quick action chips, and 100% zero emojis (clean vector drawables and typography).

---

### Official Release Packages

| Package | Target Platform | Description | File |
| :--- | :--- | :--- | :--- |
| **Android APK** | Android 8.0+ (ARM64/x86) | Production-signed APK (Scheme v2) | `OmniAgent-v0.2.0-Android.apk` |
| **Desktop (Linux)** | Linux x86_64 | Self-contained single-file binary + C++ Core engine | `OmniAgent-Desktop-linux-x64-v0.2.0.tar.gz` |
| **Desktop (Windows)** | Windows 10/11 x64 | Self-contained single-file `.exe` worker | `OmniAgent-Desktop-win-x64-v0.2.0.zip` |
| **Desktop (macOS ARM)** | macOS Apple Silicon (M1/M2/M3/M4) | Self-contained native worker | `OmniAgent-Desktop-osx-arm64-v0.2.0.tar.gz` |
| **Desktop (macOS Intel)** | macOS Intel x86_64 | Self-contained native worker | `OmniAgent-Desktop-osx-x64-v0.2.0.tar.gz` |
| **Omni Engine C++ SDK** | Linux x86_64 | C ABI headers, `libomni_engine.so`, JNI library & CMake | `omniagent-engine-linux-x64-v0.2.0.tar.gz` |
| **Python SDK Wheel** | Python 3.10+ | PyPI-ready Python Wheel | `omniagent-0.2.0-py3-none-any.whl` |
| **Python Source Dist** | Python 3.10+ | Source distribution tarball | `omniagent-0.2.0.tar.gz` |

---

### Quick Installation

#### Android Mobile
```bash
adb install -r OmniAgent-v0.2.0-Android.apk
```

#### Desktop Worker (Linux)
```bash
tar -xzf OmniAgent-Desktop-linux-x64-v0.2.0.tar.gz
cd linux-x64
./OmniAgent.Desktop --help
./OmniAgent.Desktop --watch ./dropzone
```

#### Desktop Worker (Windows)
Unzip `OmniAgent-Desktop-win-x64-v0.2.0.zip` and run `OmniAgent.Desktop.exe`.

#### Desktop Worker (macOS)
```bash
tar -xzf OmniAgent-Desktop-osx-arm64-v0.2.0.tar.gz
cd osx-arm64
./OmniAgent.Desktop --help
```

#### Python SDK
```bash
pip install omniagent-0.2.0-py3-none-any.whl
```
