"use client";

import React, { useState, useEffect } from "react";
import {
  Cpu,
  Activity,
  Sun,
  Moon,
  Terminal,
  Shield,
  Download,
} from "lucide-react";

interface HeaderProps {
  isConnected: boolean;
  theme: "light" | "dark";
  onToggleTheme: () => void;
  onOpenGetApp: () => void;
}

export const Header: React.FC<HeaderProps> = ({
  isConnected,
  theme,
  onToggleTheme,
  onOpenGetApp,
}) => {
  return (
    <header className="flex flex-col sm:flex-row justify-between items-start sm:items-center p-4 border-b border-[var(--border-subtle)] bg-[var(--bg-card)] gap-4 transition-colors">
      <div className="flex items-center gap-3">
        <div className="flex items-center justify-center w-9 h-9 rounded-lg overflow-hidden border border-[var(--border-subtle)] bg-[#121211]">
          <img
            src="/icon.png"
            alt="OmniAgent Logo"
            className="w-full h-full object-cover"
          />
        </div>
        <div>
          <div className="flex items-center gap-2">
            <h1 className="text-base font-semibold tracking-tight text-[var(--text-primary)]">
              OmniAgent Engine
            </h1>
            <span className="px-2 py-0.5 text-xs font-mono font-medium rounded bg-[var(--accent-cobalt-subtle)] text-[var(--accent-cobalt)] border border-[var(--accent-cobalt)]/20">
              v0.2.1
            </span>
          </div>
          <p className="text-xs text-[var(--text-secondary)]">
            Hybrid Local/Cloud Edge Agent Framework
          </p>
        </div>
      </div>

      <div className="flex items-center gap-3 w-full sm:w-auto justify-between sm:justify-end">
        <button
          onClick={onOpenGetApp}
          className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold rounded-md bg-[var(--accent-cobalt)] hover:bg-[var(--accent-cobalt)]/90 text-white shadow-sm border border-[var(--accent-cobalt)] transition-all hover:scale-[1.02]"
          title="Download Desktop Assistant & Android APK"
        >
          <Download className="w-3.5 h-3.5" />
          <span>Get App</span>
          <span className="ml-0.5 px-1 py-0.2 rounded bg-white/20 text-[10px] font-mono">
            v0.2.1
          </span>
        </button>

        <div className="hidden md:flex items-center gap-2 px-2.5 py-1 text-xs font-mono rounded-md border border-[var(--border-subtle)] bg-[var(--bg-surface)]">
          <Shield className="w-3.5 h-3.5 text-[var(--status-local)]" />
          <span className="text-[var(--text-secondary)]">
            Privacy Boundary:
          </span>
          <span className="text-[var(--status-local)] font-medium">
            ON-DEVICE FIRST
          </span>
        </div>

        <div className="flex items-center gap-2 px-2.5 py-1 text-xs rounded-md border border-[var(--border-subtle)] bg-[var(--bg-surface)]">
          <span
            className={`w-2 h-2 rounded-full ${isConnected ? "bg-[var(--status-local)] shadow-[0_0_8px_var(--status-local)]" : "bg-[var(--status-error)]"}`}
          />
          <span className="font-mono text-[var(--text-primary)]">
            {isConnected ? "LIVE (SSE)" : "DISCONNECTED"}
          </span>
        </div>

        <button
          onClick={onToggleTheme}
          className="p-1.5 rounded-md text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--bg-card-hover)] border border-[var(--border-subtle)] transition-colors"
          title="Toggle color theme"
          aria-label="Toggle theme"
        >
          {theme === "dark" ? (
            <Sun className="w-4 h-4" />
          ) : (
            <Moon className="w-4 h-4" />
          )}
        </button>
      </div>
    </header>
  );
};
