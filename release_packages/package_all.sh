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

# 2. Desktop Linux x64 (Native GUI Siri Assistant & Enterprise Automation)
echo "--> Packaging Desktop Linux x64 (Native GUI Application)..."
STAGING_LINUX="$REPO_ROOT/release_packages/staging/linux-x64"
rm -rf "$STAGING_LINUX"
mkdir -p "$STAGING_LINUX"

dotnet publish "$REPO_ROOT/desktop/OmniAgent.Desktop.csproj" -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o "$STAGING_LINUX"
cp "$REPO_ROOT/core/build/libomni_engine.so" "$STAGING_LINUX/" || true
cp -r "$REPO_ROOT/desktop/assets" "$STAGING_LINUX/"
cp "$REPO_ROOT/desktop/assets/omniagent.desktop" "$STAGING_LINUX/"
cp "$REPO_ROOT/scripts/install-desktop.sh" "$STAGING_LINUX/install.sh"
chmod +x "$STAGING_LINUX/install.sh"
chmod +x "$STAGING_LINUX/OmniAgent.Desktop"

cat << 'README' > "$STAGING_LINUX/README.md"
# OmniAgent Desktop GUI Assistant v0.2.1 (Linux x64)

Native Siri-like Desktop GUI Application and enterprise automation worker for Linux workstations.

## Installation
Run the installer to register OmniAgent in your Applications menu / Dash:
```bash
./install.sh
```
Once installed, search for **OmniAgent** in your GNOME/KDE Dash or launch from terminal with `omniagent`.

## Features
- **Siri-like Floating GUI**: Real-time voice orb animation, soundwave visualizer, and frosted glass dynamic HUD.
- **Hands-Free Wake Word**: Speak *"Hey Omni"* or *"OK Omni"* anytime from your desk.
- **Desktop System Automations**: Spotify playback, application launcher, volume control, screen lock, screenshot capture, timers, live weather, and system telemetry.
- **Voice Match & Accent Calibration**: Interactive GUI wizard to train your personal voice profile and accents.
- **Private On-Device Reasoning**: 100% local C++ Core SLM inference (`libomni_engine.so`).
- **CLI Fallback**: Headless support via `--say`, `--audit`, `--watch`, or `--cli`.
README

tar -czf "$DIST_DIR/OmniAgent-Desktop-linux-x64-v0.2.1.tar.gz" -C "$REPO_ROOT/release_packages/staging" linux-x64

# 3. Desktop Windows x64 (Native GUI Siri Assistant & Enterprise Automation)
echo "--> Packaging Desktop Windows x64 (Native GUI Application)..."
STAGING_WIN="$REPO_ROOT/release_packages/staging/win-x64"
rm -rf "$STAGING_WIN"
mkdir -p "$STAGING_WIN"

dotnet publish "$REPO_ROOT/desktop/OmniAgent.Desktop.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "$STAGING_WIN"
cp -r "$REPO_ROOT/desktop/assets" "$STAGING_WIN/"
cp "$REPO_ROOT/scripts/install-desktop.ps1" "$STAGING_WIN/install.ps1"

cat << 'README' > "$STAGING_WIN/README.txt"
OmniAgent Desktop GUI Assistant v0.2.1 (Windows x64)
=====================================================

Native Siri-like Desktop GUI Application for Windows 10/11 x64.

Installation:
  Right-click 'install.ps1' -> Run with PowerShell
  (This creates a Start Menu shortcut and enables background startup)

Features:
  - Siri-like Floating GUI: Animated voice orb, soundwaves, and interactive HUD.
  - Hands-Free Wake Word: Speak "Hey Omni" anytime to trigger actions.
  - System Automations: Spotify playback, open apps, volume, lock PC, screenshot.
  - Voice Match & Accent Calibration: Personal voice training across 4 phrases.
  - Private C++ SLM: 100% on-device local execution without cloud leakage.
  - CLI Fallback: Run with --say, --audit, or --watch from PowerShell/CMD.
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
