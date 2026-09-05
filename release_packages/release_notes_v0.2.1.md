## OmniAgent v0.2.1 — Desktop Assistant & Cross-Platform Voice Match Calibration

OmniAgent v0.2.1 introduces a full **Siri-like Desktop Assistant** for Windows and Linux, along with personal **Voice Match & Accent Calibration** for both Android mobile and desktop systems.

### What's New in v0.2.1:

- **Desktop Voice & System Assistant (Siri-like for PC)**:
  - Turn your Windows or Linux workstation into an autonomous voice assistant.
  - Hands-free wake word listening ("Hey Omni", "OK Omni") and interactive Cobalt HUD.
  - Audible speech synthesis (TTS via native Speech Dispatcher on Linux and SAPI on Windows).
  - Visual desktop notification toasts via `notify-send` and Windows toast notifications.
- **Extensive Desktop System Automations**:
  - **Spotify Playback & Media**: Hands-free search (`"Hey Omni, play bohemian rhapsody on spotify"`), play, pause, resume, next, and volume adjustments.
  - **Application Launcher**: `"Hey Omni, open chrome"`, `"open vscode"`, `"open terminal"`, `"open calculator"`, `"open steam"`, `"open files"`.
  - **System Controls**: Volume up/down/mute, screenshot capture (`"Hey Omni, take a screenshot"`), lock workstation (`"Hey Omni, lock screen"`).
  - **Timers, Alarms & Clock**: Background timer countdown with audio chime and alarm alert (`"Hey Omni, set a timer for 5 minutes"`), current time & date.
  - **Live Weather & Hardware Telemetry**: Instant weather (`"Hey Omni, what's the weather in London"`), CPU, RAM, and disk stats (`"how is my system doing"`).
  - **On-Device SLM Inference**: Conversational answers and general knowledge processed locally via C++ Core (`libomni_engine.so`) with zero network leaks.
- **Voice Match & Accent Calibration (Mobile & Desktop)**:
  - Guided 4-phrase calibration wizard (`--train-voice` on Desktop, or dedicated "Train Voice Match" on Android).
  - Calibrates acoustic energy fingerprints and phonetic distance to understand diverse accents and pronunciation styles.
- **Production Signed Android APK (`v0.2.1`, versionCode 5)**:
  - Zero-download on-device engine (0 MB overhead), free wake-word detection, and Phone Automation.
  - Verified with APK Signature Scheme v2.

---

### Official Release Packages

| Package | Target Platform | Description | File |
| :--- | :--- | :--- | :--- |
| **Android APK** | Android 8.0+ (ARM64/x86) | Production-signed APK (Scheme v2) | `OmniAgent-v0.2.1-Android.apk` |
| **Desktop (Linux)** | Linux x86_64 | Self-contained single-file binary + C++ Core engine | `OmniAgent-Desktop-linux-x64-v0.2.1.tar.gz` |
| **Desktop (Windows)** | Windows 10/11 x64 | Self-contained single-file `.exe` worker | `OmniAgent-Desktop-win-x64-v0.2.1.zip` |
| **Omni Engine C++ SDK** | Linux x86_64 | C ABI headers, `libomni_engine.so`, JNI library & CMake | `omniagent-engine-linux-x64-v0.2.1.tar.gz` |
| **Python SDK Wheel** | Python 3.10+ | PyPI-ready Python Wheel | `omniagent-0.2.1-py3-none-any.whl` |
| **Python Source Dist** | Python 3.10+ | Source distribution tarball | `omniagent-0.2.1.tar.gz` |

---

### Quick Installation

#### Android Mobile
```bash
adb install -r OmniAgent-v0.2.1-Android.apk
```

#### Desktop Assistant (Linux x64)
```bash
tar -xzf OmniAgent-Desktop-linux-x64-v0.2.1.tar.gz
cd linux-x64
./OmniAgent.Desktop --assistant
./OmniAgent.Desktop --say "Hey Omni, play music on spotify"
./OmniAgent.Desktop --train-voice
```

#### Desktop Assistant (Windows x64)
Unzip `OmniAgent-Desktop-win-x64-v0.2.1.zip` and run from PowerShell:
```cmd
OmniAgent.Desktop.exe --assistant
OmniAgent.Desktop.exe --say "Hey Omni, what time is it"
OmniAgent.Desktop.exe --train-voice
```

#### Python Developer SDK
```bash
pip install omniagent-0.2.1-py3-none-any.whl
```
