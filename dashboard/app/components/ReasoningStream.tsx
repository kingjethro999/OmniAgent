'use client';

import React, { useState } from 'react';
import { Activity, Trash2, Filter, Zap, CheckCircle2, AlertTriangle, ArrowUpRight } from 'lucide-react';
import { AgentEvent } from '../api/events/route';

interface ReasoningStreamProps {
  events: AgentEvent[];
  onClearEvents: () => void;
}

export const ReasoningStream: React.FC<ReasoningStreamProps> = ({ events, onClearEvents }) => {
  const [filter, setFilter] = useState<string>('ALL');

  const filteredEvents = events.filter((e) => {
    if (filter === 'ALL') return true;
    return e.event_type === filter;
  });

  const getEventBadgeColor = (type: string) => {
    switch (type) {
      case 'ROUTING':
        return 'bg-amber-500/10 text-amber-500 border-amber-500/20';
      case 'PLANNING':
        return 'bg-purple-500/10 text-purple-500 border-purple-500/20';
      case 'EXECUTING':
        return 'bg-indigo-500/10 text-indigo-500 border-indigo-500/20';
      case 'COMPLETED':
        return 'bg-emerald-500/10 text-emerald-500 border-emerald-500/20';
      case 'ERROR':
        return 'bg-red-500/10 text-red-500 border-red-500/20';
      default:
        return 'bg-zinc-500/10 text-zinc-400 border-zinc-500/20';
    }
  };

  return (
    <div className="flex flex-col h-full rounded-lg border border-[var(--border-subtle)] bg-[var(--bg-card)] p-4 gap-3">
      <div className="flex items-center justify-between pb-2 border-b border-[var(--border-subtle)]">
        <div className="flex items-center gap-2">
          <Activity className="w-4 h-4 text-[var(--accent-cobalt)]" />
          <h2 className="text-sm font-semibold text-[var(--text-primary)]">Real-Time Reasoning Stream</h2>
          <span className="text-xs text-[var(--text-tertiary)] font-mono">({filteredEvents.length} events)</span>
        </div>

        <div className="flex items-center gap-2">
          <select
            value={filter}
            onChange={(e) => setFilter(e.target.value)}
            className="px-2 py-1 text-xs rounded border border-[var(--border-subtle)] bg-[var(--bg-surface)] text-[var(--text-secondary)] focus:outline-none"
          >
            <option value="ALL">All Events</option>
            <option value="ROUTING">Routing</option>
            <option value="PLANNING">Planning</option>
            <option value="EXECUTING">Executing</option>
            <option value="COMPLETED">Completed</option>
          </select>

          <button
            onClick={onClearEvents}
            className="p-1 rounded text-[var(--text-tertiary)] hover:text-[var(--text-primary)] hover:bg-[var(--bg-card-hover)] transition-colors"
            title="Clear Event Stream"
          >
            <Trash2 className="w-4 h-4" />
          </button>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto max-h-[500px] flex flex-col gap-2.5 pr-1 font-mono text-xs">
        {filteredEvents.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-12 text-[var(--text-tertiary)] gap-2">
            <Activity className="w-6 h-6 stroke-[1.5]" />
            <p>No agent reasoning events recorded yet.</p>
          </div>
        ) : (
          filteredEvents.map((evt) => (
            <div
              key={evt.id}
              className="p-3 rounded-md border border-[var(--border-subtle)] bg-[var(--bg-surface)] hover:border-[var(--border-strong)] transition-colors"
            >
              <div className="flex items-center justify-between mb-1.5 text-[11px]">
                <div className="flex items-center gap-2">
                  <span className={`px-2 py-0.5 rounded border text-[10px] font-semibold ${getEventBadgeColor(evt.event_type)}`}>
                    {evt.event_type}
                  </span>
                  {evt.routing_decision && (
                    <span className={`px-1.5 py-0.5 rounded text-[10px] ${evt.routing_decision === 'LOCAL' ? 'bg-emerald-500/10 text-emerald-500' : 'bg-amber-500/10 text-amber-500'}`}>
                      Target: {evt.routing_decision}
                    </span>
                  )}
                </div>
                <span className="text-[var(--text-tertiary)]">
                  {new Date(evt.timestamp * 1000).toLocaleTimeString()}
                </span>
              </div>

              <div className="text-[var(--text-primary)] leading-relaxed">{evt.message}</div>

              {(evt.latency_ms || evt.complexity_score !== undefined) && (
                <div className="mt-2 pt-1.5 border-t border-[var(--border-subtle)] flex items-center gap-4 text-[10px] text-[var(--text-tertiary)]">
                  {evt.latency_ms && <span>Latency: <strong className="text-[var(--text-secondary)]">{evt.latency_ms}ms</strong></span>}
                  {evt.complexity_score !== undefined && <span>Score: <strong className="text-[var(--text-secondary)]">{evt.complexity_score}</strong></span>}
                  {evt.provider && <span>Provider: <strong className="text-[var(--text-secondary)]">{evt.provider}</strong></span>}
                </div>
              )}
            </div>
          ))
        )}
      </div>
    </div>
  );
};
