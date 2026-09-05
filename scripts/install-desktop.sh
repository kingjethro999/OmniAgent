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
mkdir -p "${INSTALL_PIXMAP_DIR}"
mkdir -p "${INSTALL_LIB_DIR}"

# 2. Build or stage release binary
echo "[1/5] Setting up OmniAgent Desktop binary..."
if [ -f "${SCRIPT_DIR}/OmniAgent.Desktop" ]; then
    cp -r "${SCRIPT_DIR}"/* "${INSTALL_LIB_DIR}/" || true
else
    cd "${REPO_ROOT}"
    dotnet publish desktop/OmniAgent.Desktop.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o "${INSTALL_LIB_DIR}"
fi

# 3. Create launcher symlink in ~/.local/bin
echo "[2/5] Creating application executable link in ${INSTALL_BIN_DIR}..."
cat << 'EOF' > "${INSTALL_BIN_DIR}/omniagent"
#!/usr/bin/env bash
exec "${HOME}/.local/share/omniagent/OmniAgent.Desktop" "$@"
EOF
chmod +x "${INSTALL_BIN_DIR}/omniagent"
chmod +x "${INSTALL_LIB_DIR}/OmniAgent.Desktop"

# 4. Install App Icons across all standard hicolor theme resolutions
echo "[3/5] Installing multi-resolution application icons..."
ICON_DIR_SRC=""
if [ -d "${SCRIPT_DIR}/assets/icons/hicolor" ]; then
    ICON_DIR_SRC="${SCRIPT_DIR}/assets/icons/hicolor"
elif [ -d "${REPO_ROOT}/desktop/assets/icons/hicolor" ]; then
    ICON_DIR_SRC="${REPO_ROOT}/desktop/assets/icons/hicolor"
fi

if [ -n "${ICON_DIR_SRC}" ] && [ -d "${ICON_DIR_SRC}" ]; then
    for size_dir in "${ICON_DIR_SRC}"/*; do
        if [ -d "${size_dir}" ]; then
            size_name="$(basename "${size_dir}")"
            target_dir="${HOME}/.local/share/icons/hicolor/${size_name}/apps"
            mkdir -p "${target_dir}"
            cp "${size_dir}/apps/omniagent.png" "${target_dir}/omniagent.png" 2>/dev/null || true
        fi
    done
fi

# Pixmap fallback
if [ -f "${REPO_ROOT}/desktop/assets/app_icon.png" ]; then
    cp "${REPO_ROOT}/desktop/assets/app_icon.png" "${INSTALL_PIXMAP_DIR}/omniagent.png"
elif [ -f "${SCRIPT_DIR}/assets/app_icon.png" ]; then
    cp "${SCRIPT_DIR}/assets/app_icon.png" "${INSTALL_PIXMAP_DIR}/omniagent.png"
fi

# Rebuild GTK Icon Theme Cache
if command -v gtk-update-icon-cache >/dev/null 2>&1; then
    gtk-update-icon-cache -f -t "${HOME}/.local/share/icons/hicolor" 2>/dev/null || true
fi

# 5. Install .desktop file
echo "[4/5] Registering desktop application entry..."
DESKTOP_SRC=""
if [ -f "${SCRIPT_DIR}/omniagent.desktop" ]; then
    DESKTOP_SRC="${SCRIPT_DIR}/omniagent.desktop"
elif [ -f "${REPO_ROOT}/desktop/assets/omniagent.desktop" ]; then
    DESKTOP_SRC="${REPO_ROOT}/desktop/assets/omniagent.desktop"
fi

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
