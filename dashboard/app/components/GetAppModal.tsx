'use client';

import React, { useState } from 'react';
import { Smartphone, Monitor, Download, Terminal, Check, Copy, ExternalLink, X, Shield, Cpu } from 'lucide-react';

interface GetAppModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const GetAppModal: React.FC<GetAppModalProps> = ({ isOpen, onClose }) => {
  const [activeTab, setActiveTab] = useState<'desktop' | 'android'>('desktop');
  const [copiedCmd, setCopiedCmd] = useState<boolean>(false);

  if (!isOpen) return null;

  const linuxInstallCmd = 'curl -sSL https://raw.githubusercontent.com/kingjethro999/OmniAgent/main/scripts/install-desktop.sh | bash';

  const handleCopyCmd = () => {
    navigator.clipboard.writeText(linuxInstallCmd);
    setCopiedCmd(true);
    setTimeout(() => setCopiedCmd(false), 2000);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/70 backdrop-blur-sm animate-in fade-in duration-200">
      <div 
        className="relative w-full max-w-2xl rounded-xl border border-[var(--border-subtle)] bg-[var(--bg-card)] shadow-2xl overflow-hidden flex flex-col max-h-[90vh]"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between p-5 border-b border-[var(--border-subtle)] bg-[var(--bg-surface)]">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-[#14161c] border border-[var(--accent-cobalt)]/40 p-1 flex items-center justify-center shadow-md">
              <img src="/icon.png" alt="OmniAgent" className="w-full h-full object-contain rounded-md" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h2 className="text-base font-bold text-[var(--text-primary)]">Get OmniAgent Applications</h2>
                <span className="px-2 py-0.5 text-[10px] font-mono font-bold rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
                  v0.2.1 Production
                </span>
              </div>
              <p className="text-xs text-[var(--text-secondary)]">Native Voice Assistant & Autonomous Edge Automation</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-lg text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--bg-card-hover)] border border-transparent hover:border-[var(--border-subtle)] transition-all"
            aria-label="Close modal"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Platform Tabs */}
        <div className="flex border-b border-[var(--border-subtle)] bg-[var(--bg-card)]">
          <button
            onClick={() => setActiveTab('desktop')}
            className={`flex-1 py-3 px-4 flex items-center justify-center gap-2 text-xs font-semibold border-b-2 transition-all ${
              activeTab === 'desktop'
                ? 'border-[var(--accent-cobalt)] text-[var(--text-primary)] bg-[var(--bg-surface)]'
                : 'border-transparent text-[var(--text-secondary)] hover:text-[var(--text-primary)]'
            }`}
          >
            <Monitor className="w-4 h-4 text-[var(--accent-cobalt)]" />
            <span>Desktop Voice Assistant (Linux / Windows)</span>
          </button>
          <button
            onClick={() => setActiveTab('android')}
            className={`flex-1 py-3 px-4 flex items-center justify-center gap-2 text-xs font-semibold border-b-2 transition-all ${
              activeTab === 'android'
                ? 'border-[var(--accent-cobalt)] text-[var(--text-primary)] bg-[var(--bg-surface)]'
                : 'border-transparent text-[var(--text-secondary)] hover:text-[var(--text-primary)]'
            }`}
          >
            <Smartphone className="w-4 h-4 text-emerald-400" />
            <span>Mobile Companion (Android APK)</span>
          </button>
        </div>

        {/* Modal Body */}
        <div className="p-6 overflow-y-auto flex-1 flex flex-col gap-5 text-xs text-[var(--text-secondary)]">
          {activeTab === 'desktop' ? (
            <div className="flex flex-col gap-4">
              <div className="p-3.5 rounded-lg border border-[var(--border-subtle)] bg-[var(--bg-surface)] flex flex-col gap-2">
                <div className="flex items-center justify-between">
                  <span className="font-semibold text-[var(--text-primary)] text-sm flex items-center gap-2">
                    <Monitor className="w-4 h-4 text-[var(--accent-cobalt)]" />
                    Linux Workstation (x64)
                  </span>
                  <span className="font-mono text-[10px] px-2 py-0.5 rounded bg-[var(--accent-cobalt-subtle)] text-[var(--accent-cobalt)]">
                    31.0 MB • Self-Contained
                  </span>
                </div>
                <p className="text-[11px] leading-relaxed">
                  Installs into your GNOME/KDE Dash with a custom squircle icon. Features a floating voice orb HUD, WebKitGTK window overlay, and full system automations.
                </p>
                
                {/* One line install command */}
                <div className="mt-1">
                  <div className="text-[10px] font-mono text-[var(--text-tertiary)] mb-1">Quick Terminal Install:</div>
                  <div className="flex items-center justify-between p-2 rounded bg-[#0e1015] border border-[#232733] font-mono text-[11px] text-indigo-200">
                    <span className="truncate mr-2">{linuxInstallCmd}</span>
                    <button
                      onClick={handleCopyCmd}
                      className="flex items-center gap-1 px-2 py-1 rounded bg-[#1c202c] hover:bg-[#282d3e] text-[var(--text-primary)] border border-indigo-500/30 transition-all flex-shrink-0"
                      title="Copy command"
                    >
                      {copiedCmd ? <Check className="w-3 h-3 text-emerald-400" /> : <Copy className="w-3 h-3" />}
                      <span className="text-[10px]">{copiedCmd ? 'Copied' : 'Copy'}</span>
                    </button>
                  </div>
                </div>

                <div className="flex items-center gap-2 mt-2">
                  <a
                    href="https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/OmniAgent-Desktop-linux-x64-v0.2.1.tar.gz"
                    target="_blank"
                    rel="noreferrer"
                    className="flex-1 py-2 px-3 rounded-lg bg-[var(--accent-cobalt)] hover:bg-[var(--accent-cobalt)]/90 text-white font-semibold text-center flex items-center justify-center gap-2 shadow-sm transition-all"
                  >
                    <Download className="w-4 h-4" />
                    <span>Download Linux .tar.gz</span>
                  </a>
                </div>
              </div>

              <div className="p-3.5 rounded-lg border border-[var(--border-subtle)] bg-[var(--bg-surface)] flex flex-col gap-2">
                <div className="flex items-center justify-between">
                  <span className="font-semibold text-[var(--text-primary)] text-sm flex items-center gap-2">
                    <Monitor className="w-4 h-4 text-blue-400" />
                    Windows 10 / 11 (x64)
                  </span>
                  <span className="font-mono text-[10px] px-2 py-0.5 rounded bg-blue-500/10 text-blue-400">
                    31.0 MB • Single-File .exe
                  </span>
                </div>
                <p className="text-[11px] leading-relaxed">
                  Single-file executable with WebView2 floating HUD, voice match calibration wizard, Start Menu registration script (<code className="font-mono text-[10px] text-blue-300">install.ps1</code>), and Spotify media controls.
                </p>
                <div className="flex items-center gap-2 mt-1">
                  <a
                    href="https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/OmniAgent-Desktop-win-x64-v0.2.1.zip"
                    target="_blank"
                    rel="noreferrer"
                    className="flex-1 py-2 px-3 rounded-lg bg-[#202533] hover:bg-[#2c3246] text-[var(--text-primary)] border border-[var(--border-subtle)] font-semibold text-center flex items-center justify-center gap-2 transition-all"
                  >
                    <Download className="w-4 h-4 text-blue-400" />
                    <span>Download Windows .zip</span>
                  </a>
                </div>
              </div>
            </div>
          ) : (
            <div className="flex flex-col gap-4">
              <div className="p-4 rounded-lg border border-[var(--border-subtle)] bg-[var(--bg-surface)] flex flex-col gap-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Smartphone className="w-5 h-5 text-emerald-400" />
                    <div>
                      <span className="font-semibold text-[var(--text-primary)] text-sm block">OmniAgent Mobile Companion</span>
                      <span className="text-[10px] text-[var(--text-tertiary)]">Android 8.0+ (API Level 26 - 34)</span>
                    </div>
                  </div>
                  <span className="font-mono text-[10px] px-2 py-0.5 rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
                    4.4 MB • Signed APK
                  </span>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-[11px]">
                  <div className="flex items-start gap-1.5">
                    <Shield className="w-3.5 h-3.5 text-emerald-400 flex-shrink-0 mt-0.5" />
                    <span>Private on-device voice assistant with C++ engine bindings</span>
                  </div>
                  <div className="flex items-start gap-1.5">
                    <Cpu className="w-3.5 h-3.5 text-emerald-400 flex-shrink-0 mt-0.5" />
                    <span>Phone automations: Alarms, Spotify, Phone Calls, SMS</span>
                  </div>
                </div>

                <div className="mt-2 flex flex-col sm:flex-row gap-2">
                  <a
                    href="https://github.com/kingjethro999/OmniAgent/releases/download/v0.2.1/OmniAgent-v0.2.1-Android.apk"
                    target="_blank"
                    rel="noreferrer"
                    className="flex-1 py-2.5 px-4 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-white font-semibold text-center flex items-center justify-center gap-2 shadow-sm transition-all"
                  >
                    <Download className="w-4 h-4" />
                    <span>Download OmniAgent-release.apk (4.4 MB)</span>
                  </a>
                </div>
              </div>

              <div className="p-3 rounded-lg border border-[var(--border-subtle)] bg-[var(--bg-card)] text-[11px] flex flex-col gap-1.5">
                <span className="font-semibold text-[var(--text-primary)]">Installation Note:</span>
                <p className="text-[var(--text-tertiary)] leading-relaxed">
                  After downloading the APK to your Android device, open your file manager and tap to install. When prompted, allow installation from your browser or file manager.
                </p>
              </div>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="p-4 border-t border-[var(--border-subtle)] bg-[var(--bg-surface)] flex items-center justify-between">
          <a
            href="https://github.com/kingjethro999/OmniAgent/releases/tag/v0.2.1"
            target="_blank"
            rel="noreferrer"
            className="flex items-center gap-1.5 text-xs text-[var(--text-secondary)] hover:text-[var(--accent-cobalt)] transition-colors"
          >
            <ExternalLink className="w-3.5 h-3.5" />
            <span>View GitHub Release v0.2.1</span>
          </a>

          <button
            onClick={onClose}
            className="px-4 py-1.5 rounded-md border border-[var(--border-subtle)] bg-[var(--bg-card)] hover:bg-[var(--bg-card-hover)] text-xs text-[var(--text-primary)] font-medium transition-colors"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
};
