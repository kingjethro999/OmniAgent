## OmniAgent v0.2.1 — Native Desktop GUI Assistant & Cross-Platform Voice Match Calibration

OmniAgent v0.2.1 delivers a complete, installable **Native Desktop GUI Assistant** (Siri-like for PC) for Windows and Linux, along with personal **Voice Match & Accent Calibration** for both Android mobile and desktop systems.

### What's New in v0.2.1:

- **Native Desktop GUI Application (Siri for PC — Not Just Terminal)**:
  - An actual installed desktop application with launcher icons in GNOME/KDE Dash and Windows Start Menu.
  - Sleek, frameless, floating glassmorphic Siri HUD overlay that floats above your active workstation windows.
  - Dynamic animated Siri voice orb with multi-layer radial gradient mesh and real-time audio soundwave frequency visualizer.
  - Hands-free wake word call-out: speak *"Hey Omni"* or *"OK Omni"* anytime from your desk to summon the assistant.
  - One-click installers: `./install.sh` (Linux) and `install.ps1` (Windows) for seamless OS installation with autostart.
- **Interactive Visual Action Cards**:
  - **Spotify Player Card**: Track artwork, real-time title/artist, play/pause toggle, skip, and volume slider.
  - **Hardware Telemetry Gauges**: Real-time progress bars for CPU load, RAM usage, and root storage.
  - **Animated Timer & Alarms**: Visual countdown clock with audio chime and alarm alert.
  - **Live Weather**: Instant temperature, forecast, and humidity for your location.
  - **System Controls**: Workstation lock, screenshot capture, and app launcher (Chrome, VS Code, Steam, Terminal).
  - **On-Device SLM Inference**: Local conversational answers processed via C++ Core (`libomni_engine.so`) with 100% privacy.
- **Voice Match & Accent Calibration (Mobile & Desktop)**:
  - Interactive 4-phrase calibration wizard directly inside the desktop GUI and Android app (Card 3B).
  - Calibrates acoustic energy fingerprints and phonetic distance to understand diverse accents and pronunciation styles.
- **Production Signed Android APK (`v0.2.1`, versionCode 5)**:
  - Zero-download on-device engine (0 MB overhead), pure Java AudioRecord buffer pipeline, and Phone Automation.
  - Verified with APK Signature Scheme v2.

---

### Official Release Packages

| Package | Target Platform | Description | File |
| :--- | :--- | :--- | :--- |
| **Android APK** | Android 8.0+ (ARM64/x86) | Production-signed APK (Scheme v2) | `OmniAgent-v0.2.1-Android.apk` |
| **Desktop GUI (Linux)** | Linux x86_64 | Native GUI Application + installer + C++ Core | `OmniAgent-Desktop-linux-x64-v0.2.1.tar.gz` |
| **Desktop GUI (Windows)** | Windows 10/11 x64 | Native GUI Application + installer (`.exe`) | `OmniAgent-Desktop-win-x64-v0.2.1.zip` |
| **Omni Engine C++ SDK** | Linux x86_64 | C ABI headers, `libomni_engine.so`, JNI library & CMake | `omniagent-engine-linux-x64-v0.2.1.tar.gz` |
| **Python SDK Wheel** | Python 3.10+ | PyPI-ready Python Wheel | `omniagent-0.2.1-py3-none-any.whl` |
| **Python Source Dist** | Python 3.10+ | Source distribution tarball | `omniagent-0.2.1.tar.gz` |

---

### Quick Installation

#### Desktop Assistant Application (Linux x64)
Extract and run the one-click installer to register in your Applications menu / Dash:
```bash
tar -xzf OmniAgent-Desktop-linux-x64-v0.2.1.tar.gz
cd linux-x64
./install.sh
```
Search for **OmniAgent** in your application launcher or click the app icon. Speak *"Hey Omni"* anytime!

#### Desktop Assistant Application (Windows x64)
Unzip `OmniAgent-Desktop-win-x64-v0.2.1.zip` and run the installer:
```powershell
.\win-x64\install.ps1
```
OmniAgent is registered in your Start Menu and ready for hands-free voice commands.

#### Android Mobile
```bash
adb install -r OmniAgent-v0.2.1-Android.apk
```

#### Python Developer SDK
```bash
pip install omniagent-0.2.1-py3-none-any.whl
```
