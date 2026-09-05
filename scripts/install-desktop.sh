#!/usr/bin/env bash
set -euo pipefail

# ══════════════════════════════════════════════════════════════════════════
# OmniAgent Desktop Assistant — Linux System Application Installer
# Installs OmniAgent as a native GUI application in GNOME/KDE Dash
# ══════════════════════════════════════════════════════════════════════════

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

INSTALL_BIN_DIR="${HOME}/.local/bin"
INSTALL_APP_DIR="${HOME}/.local/share/applications"
INSTALL_ICON_DIR="${HOME}/.local/share/icons/hicolor/512x512/apps"
INSTALL_PIXMAP_DIR="${HOME}/.local/share/pixmaps"
INSTALL_LIB_DIR="${HOME}/.local/share/omniagent"
AUTOSTART_DIR="${HOME}/.config/autostart"

echo "=========================================================="
echo "  OmniAgent Desktop Assistant — Linux Application Installer"
echo "=========================================================="
echo ""

# 1. Ensure required directories exist
mkdir -p "${INSTALL_BIN_DIR}"
mkdir -p "${INSTALL_APP_DIR}"
mkdir -p "${INSTALL_ICON_DIR}"
mkdir -p "${INSTALL_PIXMAP_DIR}"
mkdir -p "${INSTALL_LIB_DIR}"

# 2. Build release binary if not built
echo "[1/5] Building OmniAgent Desktop Release..."
cd "${REPO_ROOT}"
dotnet publish desktop/OmniAgent.Desktop.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o "${INSTALL_LIB_DIR}"

# 3. Create launcher symlink in ~/.local/bin
echo "[2/5] Creating application executable link in ${INSTALL_BIN_DIR}..."
cat << 'EOF' > "${INSTALL_BIN_DIR}/omniagent"
#!/usr/bin/env bash
exec "${HOME}/.local/share/omniagent/OmniAgent.Desktop" "$@"
EOF
chmod +x "${INSTALL_BIN_DIR}/omniagent"
chmod +x "${INSTALL_LIB_DIR}/OmniAgent.Desktop"

# 4. Install App Icons
echo "[3/5] Installing application icons..."
ICON_SRC="${REPO_ROOT}/desktop/assets/app_icon.png"
if [ -f "${ICON_SRC}" ]; then
    cp "${ICON_SRC}" "${INSTALL_ICON_DIR}/omniagent.png"
    cp "${ICON_SRC}" "${INSTALL_PIXMAP_DIR}/omniagent.png"
fi

# 5. Install .desktop file
echo "[4/5] Registering desktop application entry..."
DESKTOP_SRC="${REPO_ROOT}/desktop/assets/omniagent.desktop"
cp "${DESKTOP_SRC}" "${INSTALL_APP_DIR}/omniagent.desktop"

# Fix Exec path to absolute if ~/.local/bin is not in default GUI path
sed -i "s|Exec=omniagent|Exec=${INSTALL_BIN_DIR}/omniagent|g" "${INSTALL_APP_DIR}/omniagent.desktop"

# Update desktop database
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database "${INSTALL_APP_DIR}" 2>/dev/null || true
fi

# 6. Autostart setup
echo "[5/5] Configuring optional background autostart..."
mkdir -p "${AUTOSTART_DIR}"
cp "${INSTALL_APP_DIR}/omniagent.desktop" "${AUTOSTART_DIR}/omniagent.desktop"

echo ""
echo "=========================================================="
echo "  Installation Successful!"
echo "=========================================================="
echo "OmniAgent Assistant is now installed as a desktop application."
echo ""
echo "How to use:"
echo "  1. Search for 'OmniAgent' in your Applications menu / Dash."
echo "  2. Run 'omniagent' from any terminal."
echo "  3. Speak 'Hey Omni' to trigger commands anytime!"
echo "=========================================================="
