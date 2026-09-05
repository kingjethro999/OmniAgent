# ══════════════════════════════════════════════════════════════════════════
# OmniAgent Desktop Assistant — Windows System Application Installer
# Installs OmniAgent as a native application in Windows Start Menu & Startup
# ══════════════════════════════════════════════════════════════════════════

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

$installDir = Join-Path $env:LOCALAPPDATA "OmniAgent"
$programsDir = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs"
$startupDir = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\Startup"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  OmniAgent Desktop Assistant — Windows Installer" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Ensure target directory exists
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

# 2. Build or copy release files
Write-Host "[1/4] Publishing Windows Release..." -ForegroundColor Yellow
Set-Location $repoRoot
dotnet publish desktop/OmniAgent.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $installDir

# 3. Create Start Menu Shortcut
Write-Host "[2/4] Registering in Windows Start Menu..." -ForegroundColor Yellow
$wshShell = New-Object -ComObject WScript.Shell
$shortcutPath = Join-Path $programsDir "OmniAgent.lnk"
$shortcut = $wshShell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = Join-Path $installDir "OmniAgent.Desktop.exe"
$shortcut.WorkingDirectory = $installDir
$shortcut.Description = "Siri-like Desktop Voice Assistant for PC"
$iconPath = Join-Path $installDir "assets\app_icon.ico"
if (Test-Path $iconPath) {
    $shortcut.IconLocation = $iconPath
}
$shortcut.Save()

# 4. Create Startup Shortcut (Optional auto-start on login)
Write-Host "[3/4] Adding to Windows Startup folder..." -ForegroundColor Yellow
$startupShortcutPath = Join-Path $startupDir "OmniAgent.lnk"
$startupShortcut = $wshShell.CreateShortcut($startupShortcutPath)
$startupShortcut.TargetPath = Join-Path $installDir "OmniAgent.Desktop.exe"
$startupShortcut.WorkingDirectory = $installDir
$startupShortcut.Description = "OmniAgent Background Voice Assistant"
if (Test-Path $iconPath) {
    $startupShortcut.IconLocation = $iconPath
}
$startupShortcut.Save()

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "  Installation Successful!" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "OmniAgent Assistant is now installed as a desktop application."
Write-Host "You can find it in your Windows Start Menu under 'OmniAgent'."
Write-Host "Speak 'Hey Omni' anytime to trigger assistant actions!"
