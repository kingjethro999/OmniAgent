"use client";

import React from "react";
import {
  Code,
  Cpu,
  Smartphone,
  Monitor,
  CheckCircle2,
  Circle,
} from "lucide-react";

export const ArchitecturePanel: React.FC = () => {
  const phases = [
    {
      num: "Phase 1",
      title: "Agent Brain & Router",
      stack: "Python / Jupyter SDK",
      status: "Active / Fully Operational",
      done: true,
      desc: "Task complexity classification, local vs cloud routing, plan step decomposition, and tool execution registry.",
    },
    {
      num: "Phase 2",
      title: "Native Inference Muscle",
      stack: "C / C++ (Vulkan / WebGPU)",
      status: "C API Header Scaffolded",
      done: true,
      desc: "Low-level GGUF model loader, hardware memory pool, tokenization library, and C ABI dynamic export (omni_engine.so/dll).",
    },
    {
      num: "Phase 3",
      title: "Platform Runtime Shells",
      stack: "C# (.NET) & Java (Android)",
      status: "Project Shells Initialized",
      done: true,
      desc: "P/Invoke background worker for Windows/macOS and JNI Android foreground service for on-device mobile AI.",
    },
    {
      num: "Phase 4",
      title: "Control Panel Dashboard",
      stack: "Next.js 16 + React 19 + SSE",
      status: "Active / Connected",
      done: true,
      desc: "Real-time WebSocket/SSE telemetry, interactive task simulator, privacy boundary monitor, and live reasoning feed.",
    },
  ];

  return (
    <div className="p-4 rounded-lg border border-[var(--border-subtle)] bg-[var(--bg-card)]">
      <div className="flex items-center gap-2 mb-3 pb-2 border-b border-[var(--border-subtle)]">
        <Monitor className="w-4 h-4 text-[var(--accent-cobalt)]" />
        <h2 className="text-sm font-semibold text-[var(--text-primary)]">
          OmniAgent Engine Architecture & Roadmap
        </h2>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 text-xs">
        {phases.map((p) => (
          <div
            key={p.num}
            className="p-3 rounded-md border border-[var(--border-subtle)] bg-[var(--bg-surface)]"
          >
            <div className="flex items-center justify-between mb-1">
              <span className="font-mono font-semibold text-[var(--accent-cobalt)]">
                {p.num}
              </span>
              <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500" />
            </div>
            <h3 className="font-semibold text-[var(--text-primary)] mb-0.5">
              {p.title}
            </h3>
            <span className="text-[11px] font-mono text-[var(--text-tertiary)] block mb-1.5">
              {p.stack}
            </span>
            <p className="text-[11px] text-[var(--text-secondary)] leading-relaxed">
              {p.desc}
            </p>
          </div>
        ))}
      </div>
    </div>
  );
};
