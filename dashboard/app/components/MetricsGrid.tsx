'use client';

import React from 'react';
import { Zap, DollarSign, Cpu, Layers } from 'lucide-react';

interface MetricsGridProps {
  localRatio: number;
  cloudRatio: number;
  avgLatencyMs: number;
  costSavedUsd: number;
}

export const MetricsGrid: React.FC<MetricsGridProps> = ({
  localRatio,
  cloudRatio,
  avgLatencyMs,
  costSavedUsd,
}) => {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 p-4">
      <div className="p-3.5 rounded-lg border border-[var(--border-subtle)] bg-[var(--bg-card)] hover:border-[var(--border-strong)] transition-colors">
        <div className="flex items-center justify-between text-xs text-[var(--text-secondary)] mb-1">
          <span>Routing Execution Ratio</span>
          <Zap className="w-4 h-4 text-[var(--accent-cobalt)]" />
        </div>
        <div className="text-xl font-bold font-mono text-[var(--text-primary)]">
          {localRatio}% <span className="text-xs font-normal text-[var(--text-secondary)]">Local</span> / {cloudRatio}% <span className="text-xs font-normal text-[var(--text-secondary)]">Cloud</span>
        </div>
        <div className="w-full bg-[var(--bg-surface)] h-1.5 rounded-full mt-2 overflow-hidden flex">
          <div className="bg-[var(--status-local)] h-full" style={{ width: `${localRatio}%` }} />
          <div className="bg-[var(--status-cloud)] h-full" style={{ width: `${cloudRatio}%` }} />
        </div>
      </div>

      <div className="p-3.5 rounded-lg border border-[var(--border-subtle)] bg-[var(--bg-card)] hover:border-[var(--border-strong)] transition-colors">
        <div className="flex items-center justify-between text-xs text-[var(--text-secondary)] mb-1">
          <span>On-Device Inference Latency</span>
          <Cpu className="w-4 h-4 text-[var(--status-local)]" />
        </div>
        <div className="text-xl font-bold font-mono text-[var(--text-primary)]">
          {avgLatencyMs} <span className="text-xs font-normal text-[var(--text-secondary)]">ms</span>
        </div>
        <p className="text-[11px] text-[var(--text-tertiary)] mt-1">C++ GGML / Vulkan Hardware Accelerated</p>
      </div>

      <div className="p-3.5 rounded-lg border border-[var(--border-subtle)] bg-[var(--bg-card)] hover:border-[var(--border-strong)] transition-colors">
        <div className="flex items-center justify-between text-xs text-[var(--text-secondary)] mb-1">
          <span>Cloud Offloading Savings</span>
          <DollarSign className="w-4 h-4 text-[var(--status-cloud)]" />
        </div>
        <div className="text-xl font-bold font-mono text-[var(--text-primary)]">
          ${costSavedUsd.toFixed(2)}
        </div>
        <p className="text-[11px] text-[var(--text-tertiary)] mt-1">Saved vs 100% Cloud API calls</p>
      </div>

      <div className="p-3.5 rounded-lg border border-[var(--border-subtle)] bg-[var(--bg-card)] hover:border-[var(--border-strong)] transition-colors">
        <div className="flex items-center justify-between text-xs text-[var(--text-secondary)] mb-1">
          <span>Native Core Stack</span>
          <Layers className="w-4 h-4 text-[var(--accent-cobalt)]" />
        </div>
        <div className="text-sm font-semibold font-mono text-[var(--text-primary)]">
          C++ / C# / Java / Python
        </div>
        <p className="text-[11px] text-[var(--text-tertiary)] mt-1">Unified Dynamic Library Binding</p>
      </div>
    </div>
  );
};
