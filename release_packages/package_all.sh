#!/usr/bin/env bash
set -e

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

echo "=== Preparing Release Packages for OmniAgent v0.2.0 ==="
DIST_DIR="$REPO_ROOT/release_packages/dist"
rm -rf "$DIST_DIR"
mkdir -p "$DIST_DIR"

# 1. Android APK
echo "--> Staging Android APK..."
cp "$REPO_ROOT/mobile/build/outputs/apk/release/OmniAgentMobile-release.apk" "$DIST_DIR/OmniAgent-v0.2.0-Android.apk"

# 2. Desktop Linux x64
echo "--> Packaging Desktop Linux x64..."
STAGING_LINUX="$REPO_ROOT/release_packages/staging/linux-x64"
cp "$REPO_ROOT/release_packages/desktop/linux-x64/OmniAgent.Desktop" "$STAGING_LINUX/"
cp "$REPO_ROOT/core/build/libomni_engine.so" "$STAGING_LINUX/"
cp -r "$REPO_ROOT/desktop/assets" "$STAGING_LINUX/"
cat << 'README' > "$STAGING_LINUX/README.md"
# OmniAgent Desktop Worker v0.2.0 (Linux x64)

Standalone native enterprise worker with bundled C++ Core inference engine (`libomni_engine.so`).

## Quick Start
```bash
chmod +x OmniAgent.Desktop
./OmniAgent.Desktop --help
./OmniAgent.Desktop --watch ./dropzone
./OmniAgent.Desktop --audit /path/to/docs
```
README
tar -czf "$DIST_DIR/OmniAgent-Desktop-linux-x64-v0.2.0.tar.gz" -C "$REPO_ROOT/release_packages/staging" linux-x64

# 3. Desktop Windows x64
echo "--> Packaging Desktop Windows x64..."
STAGING_WIN="$REPO_ROOT/release_packages/staging/win-x64"
cp "$REPO_ROOT/release_packages/desktop/win-x64/OmniAgent.Desktop.exe" "$STAGING_WIN/"
cp -r "$REPO_ROOT/desktop/assets" "$STAGING_WIN/"
cat << 'README' > "$STAGING_WIN/README.txt"
OmniAgent Desktop Worker v0.2.0 (Windows x64)
=============================================

Standalone native enterprise worker for Windows 10/11 x64.

Usage:
  OmniAgent.Desktop.exe --help
  OmniAgent.Desktop.exe --watch C:\dropzone
  OmniAgent.Desktop.exe --audit C:\documents
README
(cd "$REPO_ROOT/release_packages/staging" && zip -r "$DIST_DIR/OmniAgent-Desktop-win-x64-v0.2.0.zip" win-x64)

# 4. Desktop macOS ARM64 (Apple Silicon)
echo "--> Packaging Desktop macOS ARM64..."
STAGING_OSX_ARM="$REPO_ROOT/release_packages/staging/osx-arm64"
cp "$REPO_ROOT/release_packages/desktop/osx-arm64/OmniAgent.Desktop" "$STAGING_OSX_ARM/"
cp -r "$REPO_ROOT/desktop/assets" "$STAGING_OSX_ARM/"
cat << 'README' > "$STAGING_OSX_ARM/README.md"
# OmniAgent Desktop Worker v0.2.0 (macOS Apple Silicon arm64)

Standalone native enterprise worker for macOS 12+ (M1/M2/M3/M4).

## Quick Start
```bash
chmod +x OmniAgent.Desktop
./OmniAgent.Desktop --help
./OmniAgent.Desktop --watch ./dropzone
./OmniAgent.Desktop --audit /path/to/docs
```
README
tar -czf "$DIST_DIR/OmniAgent-Desktop-osx-arm64-v0.2.0.tar.gz" -C "$REPO_ROOT/release_packages/staging" osx-arm64

# 5. Desktop macOS Intel x64
echo "--> Packaging Desktop macOS Intel x64..."
STAGING_OSX_X64="$REPO_ROOT/release_packages/staging/osx-x64"
cp "$REPO_ROOT/release_packages/desktop/osx-x64/OmniAgent.Desktop" "$STAGING_OSX_X64/"
cp -r "$REPO_ROOT/desktop/assets" "$STAGING_OSX_X64/"
cat << 'README' > "$STAGING_OSX_X64/README.md"
# OmniAgent Desktop Worker v0.2.0 (macOS Intel x64)

Standalone native enterprise worker for macOS Intel x86_64.

## Quick Start
```bash
chmod +x OmniAgent.Desktop
./OmniAgent.Desktop --help
./OmniAgent.Desktop --watch ./dropzone
./OmniAgent.Desktop --audit /path/to/docs
```
README
tar -czf "$DIST_DIR/OmniAgent-Desktop-osx-x64-v0.2.0.tar.gz" -C "$REPO_ROOT/release_packages/staging" osx-x64

# 6. Main Omni Engine Core (C++ SDK & Shared Libraries)
echo "--> Packaging Main Omni Engine C++ SDK..."
STAGING_ENGINE="$REPO_ROOT/release_packages/staging/omniagent-engine-linux-x64"
cp "$REPO_ROOT/core/include/omni_engine.h" "$STAGING_ENGINE/include/"
cp "$REPO_ROOT/core/build/libomni_engine.so" "$STAGING_ENGINE/lib/"
cp "$REPO_ROOT/core/build/libomni_engine_jni.so" "$STAGING_ENGINE/lib/"
cp "$REPO_ROOT/core/CMakeLists.txt" "$STAGING_ENGINE/"
cat << 'README' > "$STAGING_ENGINE/README.md"
# OmniAgent Core Inference Engine v0.2.0 (C++ / JNI SDK)

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
tar -czf "$DIST_DIR/omniagent-engine-linux-x64-v0.2.0.tar.gz" -C "$REPO_ROOT/release_packages/staging" omniagent-engine-linux-x64

# 7. Python SDK (Wheel & Source Dist)
echo "--> Copying Python SDK Packages..."
cp "$REPO_ROOT/agent/dist/omniagent-0.2.0-py3-none-any.whl" "$DIST_DIR/"
cp "$REPO_ROOT/agent/dist/omniagent-0.2.0.tar.gz" "$DIST_DIR/"

echo "=== All Packages Successfully Staged in $DIST_DIR ==="
ls -lh "$DIST_DIR"
