"use client";

import React, { useState } from "react";
import {
  Play,
  Sparkles,
  Server,
  Terminal,
  CheckCircle2,
  ShieldCheck,
  FileText,
  Code,
} from "lucide-react";

interface AgentWorkbenchProps {
  onDispatchTask: (task: string) => Promise<void>;
  isDispatching: boolean;
}

export const AgentWorkbench: React.FC<AgentWorkbenchProps> = ({
  onDispatchTask,
  isDispatching,
}) => {
  const [prompt, setPrompt] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!prompt.trim() || isDispatching) return;
    await onDispatchTask(prompt.trim());
    setPrompt("");
  };

  const handlePreset = (presetText: string) => {
    setPrompt(presetText);
  };

  return (
    <div className="flex flex-col gap-4">
      {/* Active Agent State Card */}
      <div className="p-4 rounded-lg border border-[var(--border-subtle)] bg-[var(--bg-card)]">
        <div className="flex items-center justify-between mb-3 pb-2 border-b border-[var(--border-subtle)]">
          <div className="flex items-center gap-2">
            <Server className="w-4 h-4 text-[var(--accent-cobalt)]" />
            <h2 className="text-sm font-semibold text-[var(--text-primary)]">
              Active Runtime Worker
            </h2>
          </div>
          <span className="inline-flex items-center gap-1.5 px-2 py-0.5 text-xs font-mono rounded bg-emerald-500/10 text-emerald-500 border border-emerald-500/20">
            <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
            READY (IDLE)
          </span>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
          <div className="p-2.5 rounded bg-[var(--bg-surface)] border border-[var(--border-subtle)]">
            <span className="text-[var(--text-tertiary)] block mb-0.5">
              Local Engine (Phase 2):
            </span>
            <span className="font-mono text-[var(--text-primary)] font-medium">
              Phi-4-mini (C++ / Vulkan)
            </span>
          </div>
          <div className="p-2.5 rounded bg-[var(--bg-surface)] border border-[var(--border-subtle)]">
            <span className="text-[var(--text-tertiary)] block mb-0.5">
              Cloud Adapter (Phase 1):
            </span>
            <span className="font-mono text-[var(--text-primary)] font-medium">
              GPT-4o / Gemini Flash
            </span>
          </div>
        </div>
      </div>

      {/* Task Simulator / Prompt Form */}
      <div className="p-4 rounded-lg border border-[var(--border-subtle)] bg-[var(--bg-card)]">
        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center gap-2">
            <Terminal className="w-4 h-4 text-[var(--accent-cobalt)]" />
            <h2 className="text-sm font-semibold text-[var(--text-primary)]">
              Task Simulator & Dispatcher
            </h2>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          <textarea
            value={prompt}
            onChange={(e) => setPrompt(e.target.value)}
            placeholder="Type a task prompt to test task routing & plan execution..."
            rows={3}
            className="w-full p-3 text-xs font-mono rounded-md border border-[var(--border-subtle)] bg-[var(--bg-surface)] text-[var(--text-primary)] focus:outline-none focus:border-[var(--accent-cobalt)] transition-colors"
          />

          <div className="flex flex-wrap items-center justify-between gap-2">
            <div className="flex items-center gap-1.5">
              <span className="text-[11px] text-[var(--text-tertiary)]">
                Presets:
              </span>
              <button
                type="button"
                onClick={() =>
                  handlePreset(
                    "Summarize my local text files and search recent logs.",
                  )
                }
                className="px-2 py-1 text-[11px] rounded bg-[var(--bg-surface)] border border-[var(--border-subtle)] hover:bg-[var(--bg-card-hover)] text-[var(--text-secondary)] transition-colors"
              >
                Local Summary
              </button>
              <button
                type="button"
                onClick={() =>
                  handlePreset(
                    "Calculate the matrix determinant and synthesize deep research essay.",
                  )
                }
                className="px-2 py-1 text-[11px] rounded bg-[var(--bg-surface)] border border-[var(--border-subtle)] hover:bg-[var(--bg-card-hover)] text-[var(--text-secondary)] transition-colors"
              >
                Cloud Math
              </button>
            </div>

            <button
              type="submit"
              disabled={isDispatching || !prompt.trim()}
              className="inline-flex items-center gap-1.5 px-4 py-1.5 text-xs font-medium rounded-md bg-[var(--accent-cobalt)] hover:bg-[var(--accent-cobalt-hover)] text-white disabled:opacity-50 transition-colors"
            >
              <Play className="w-3.5 h-3.5 fill-current" />
              {isDispatching ? "Dispatching..." : "Dispatch Task"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
