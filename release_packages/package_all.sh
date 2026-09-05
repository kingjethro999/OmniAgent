#!/usr/bin/env bash
set -e

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

echo "=== Preparing Release Packages for OmniAgent v0.2.1 ==="
DIST_DIR="$REPO_ROOT/release_packages/dist"
rm -rf "$DIST_DIR"
mkdir -p "$DIST_DIR"

# 1. Android APK
echo "--> Staging Android APK..."
cp "$REPO_ROOT/mobile/build/outputs/apk/release/OmniAgentMobile-release.apk" "$DIST_DIR/OmniAgent-v0.2.1-Android.apk"
cp "$REPO_ROOT/mobile/build/outputs/apk/release/OmniAgentMobile-release.apk" "$REPO_ROOT/OmniAgent-v0.2.1-Android.apk"
cp "$REPO_ROOT/mobile/build/outputs/apk/release/OmniAgentMobile-release.apk" "$REPO_ROOT/OmniAgent-release.apk"

# 2. Desktop Linux x64 (Siri-like Voice Assistant & Enterprise Automation)
echo "--> Packaging Desktop Linux x64..."
STAGING_LINUX="$REPO_ROOT/release_packages/staging/linux-x64"
rm -rf "$STAGING_LINUX"
mkdir -p "$STAGING_LINUX"
cp "$REPO_ROOT/release_packages/desktop/linux-x64/OmniAgent.Desktop" "$STAGING_LINUX/"
cp "$REPO_ROOT/core/build/libomni_engine.so" "$STAGING_LINUX/"
cp -r "$REPO_ROOT/desktop/assets" "$STAGING_LINUX/"
cat << 'README' > "$STAGING_LINUX/README.md"
# OmniAgent Desktop Worker & Siri-like Assistant v0.2.1 (Linux x64)

Standalone native Linux assistant and enterprise automation worker with bundled C++ Core inference engine (`libomni_engine.so`).

## Features
- **Siri-like Voice Assistant**: Hands-free wake-word detection ("Hey Omni", "OK Omni") with speech output (TTS via `spd-say`) and desktop notifications (`notify-send`).
- **Voice Match & Accent Calibration**: Adapts to user vocal tone, pitch, and natural regional accents (`--train-voice`).
- **Desktop System Automations**: Spotify playback & media controls, app launcher (Chrome, VS Code, Terminal, etc.), volume controls, screenshot capture, lock screen, weather, system telemetry, and timers/alarms.
- **Privacy & Security**: 100% on-device code & document auditing with zero data leaving the workstation.

## Quick Start
```bash
chmod +x OmniAgent.Desktop

# 1. Launch Interactive Siri Assistant HUD
./OmniAgent.Desktop --assistant

# 2. Run a direct voice command with speech output
./OmniAgent.Desktop --say "Hey Omni, play the box by roddy rich on spotify"
./OmniAgent.Desktop --say "Hey Omni, what is the weather in London"
./OmniAgent.Desktop --say "Hey Omni, how is my system doing"

# 3. Train your voice & accent profile
./OmniAgent.Desktop --train-voice

# 4. Enterprise Dropzone Watcher & Document Auditing
./OmniAgent.Desktop --watch ./dropzone
./OmniAgent.Desktop --audit /path/to/docs
```
README
tar -czf "$DIST_DIR/OmniAgent-Desktop-linux-x64-v0.2.1.tar.gz" -C "$REPO_ROOT/release_packages/staging" linux-x64

# 3. Desktop Windows x64 (Siri-like Voice Assistant & Enterprise Automation)
echo "--> Packaging Desktop Windows x64..."
STAGING_WIN="$REPO_ROOT/release_packages/staging/win-x64"
rm -rf "$STAGING_WIN"
mkdir -p "$STAGING_WIN"
cp "$REPO_ROOT/release_packages/desktop/win-x64/OmniAgent.Desktop.exe" "$STAGING_WIN/"
cp -r "$REPO_ROOT/desktop/assets" "$STAGING_WIN/"
cat << 'README' > "$STAGING_WIN/README.txt"
OmniAgent Desktop Worker & Siri-like Assistant v0.2.1 (Windows x64)
===================================================================

Standalone native assistant and enterprise automation worker for Windows 10/11 x64.

Features:
- Siri-like Voice Assistant: SAPI Text-to-Speech, toast notifications, wake-word detection ("Hey Omni").
- Voice Match & Accent Calibration: Adapts to your voice and pronunciation across 4 quick prompts.
- Windows System Automations: Spotify playback, media controls, app launching (Chrome, Code, Notepad, Calc), volume control, lock workstation, screenshot capture, timers/alarms, and weather.
- 100% On-Device Privacy: Local code auditing and C++ SLM execution without cloud dependencies.

Usage:
  OmniAgent.Desktop.exe --assistant
  OmniAgent.Desktop.exe --say "Hey Omni, play music on spotify"
  OmniAgent.Desktop.exe --say "Hey Omni, what time is it"
  OmniAgent.Desktop.exe --train-voice
  OmniAgent.Desktop.exe --watch C:\dropzone
  OmniAgent.Desktop.exe --audit C:\documents
README
(cd "$REPO_ROOT/release_packages/staging" && zip -r "$DIST_DIR/OmniAgent-Desktop-win-x64-v0.2.1.zip" win-x64)

# 4. Main Omni Engine Core (C++ SDK & Shared Libraries)
echo "--> Packaging Main Omni Engine C++ SDK..."
STAGING_ENGINE="$REPO_ROOT/release_packages/staging/omniagent-engine-linux-x64"
rm -rf "$STAGING_ENGINE"
mkdir -p "$STAGING_ENGINE/include" "$STAGING_ENGINE/lib"
cp "$REPO_ROOT/core/include/omni_engine.h" "$STAGING_ENGINE/include/"
cp "$REPO_ROOT/core/build/libomni_engine.so" "$STAGING_ENGINE/lib/"
cp "$REPO_ROOT/core/build/libomni_engine_jni.so" "$STAGING_ENGINE/lib/"
cp "$REPO_ROOT/core/CMakeLists.txt" "$STAGING_ENGINE/"
cat << 'README' > "$STAGING_ENGINE/README.md"
# OmniAgent Core Inference Engine v0.2.1 (C++ / JNI SDK)

High-performance native C++ edge inference engine library with C ABI and JNI bindings.

## Contents
- `include/omni_engine.h`: C ABI headers for C, C++, C# (P/Invoke), and Rust FFI.
- `lib/libomni_engine.so`: Compiled native engine shared library.
- `lib/libomni_engine_jni.so`: Compiled Java Native Interface (JNI) bridge for Android / JVM.
- `CMakeLists.txt`: CMake build configuration.

## Linking
Compile with:
```bash
gcc -Iinclude -Llib main.c -lomni_engine -Wl,-rpath,'$ORIGIN/lib' -o main
```
README
tar -czf "$DIST_DIR/omniagent-engine-linux-x64-v0.2.1.tar.gz" -C "$REPO_ROOT/release_packages/staging" omniagent-engine-linux-x64

# 5. Python SDK (Wheel & Source Dist)
echo "--> Copying Python SDK Packages..."
cp "$REPO_ROOT/agent/dist/omniagent-0.2.1-py3-none-any.whl" "$DIST_DIR/"
cp "$REPO_ROOT/agent/dist/omniagent-0.2.1.tar.gz" "$DIST_DIR/"

echo "=== All Packages Successfully Staged in $DIST_DIR ==="
ls -lh "$DIST_DIR"
