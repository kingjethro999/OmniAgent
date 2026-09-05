'use client';

import React, { useState, useEffect } from 'react';
import { Header } from './components/Header';
import { MetricsGrid } from './components/MetricsGrid';
import { AgentWorkbench } from './components/AgentWorkbench';
import { ReasoningStream } from './components/ReasoningStream';
import { ArchitecturePanel } from './components/ArchitecturePanel';
import { GetAppModal } from './components/GetAppModal';
import { AgentEvent } from '@/lib/eventsStore';
import { Monitor, Smartphone, Download } from 'lucide-react';

export default function Home() {
  const [theme, setTheme] = useState<'light' | 'dark'>('dark');
  const [isConnected, setIsConnected] = useState<boolean>(false);
  const [events, setEvents] = useState<AgentEvent[]>([]);
  const [isDispatching, setIsDispatching] = useState<boolean>(false);
  const [isGetAppOpen, setIsGetAppOpen] = useState<boolean>(false);
  const [metrics, setMetrics] = useState({
    localTasksRatio: 84,
    cloudTasksRatio: 16,
    avgLatencyMs: 38,
    costSavedUsd: 14.85,
  });

  // Apply theme class to document
  useEffect(() => {
    if (theme === 'dark') {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }, [theme]);

  // Connect to SSE stream
  useEffect(() => {
    let eventSource: EventSource | null = null;

    try {
      eventSource = new EventSource('/api/stream');

      eventSource.onopen = () => {
        setIsConnected(true);
      };

      eventSource.onmessage = (e) => {
        try {
          const payload = JSON.parse(e.data);
          if (payload.type === 'INIT') {
            setEvents(payload.events || []);
          } else if (payload.type === 'EVENT' && payload.event) {
            setEvents((prev) => [payload.event, ...prev]);
          }
        } catch (err) {
          console.error('SSE parse error:', err);
        }
      };

      eventSource.onerror = () => {
        setIsConnected(false);
      };
    } catch (err) {
      setIsConnected(false);
    }

    return () => {
      if (eventSource) eventSource.close();
    };
  }, []);

  const toggleTheme = () => {
    setTheme((prev) => (prev === 'dark' ? 'light' : 'dark'));
  };

  const handleDispatchTask = async (taskText: string) => {
    setIsDispatching(true);
    try {
      // 1. Post THINKING event
      await fetch('/api/events', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          event_type: 'THINKING',
          message: `Received task dispatch: "${taskText}"`,
        }),
      });

      // 2. Simulate Router classification (Local vs Cloud)
      const isLocal = !taskText.toLowerCase().includes('essay') && !taskText.toLowerCase().includes('matrix') && !taskText.toLowerCase().includes('math');
      const decision = isLocal ? 'LOCAL' : 'CLOUD';
      const score = isLocal ? 0.28 : 0.85;

      await new Promise((r) => setTimeout(r, 600));
      await fetch('/api/events', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          event_type: 'ROUTING',
          message: `Task Router evaluated prompt. Classified target: ${decision}`,
          routing_decision: decision,
          complexity_score: score,
        }),
      });

      // 3. Simulate Planning step
      await new Promise((r) => setTimeout(r, 500));
      await fetch('/api/events', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          event_type: 'PLANNING',
          message: `TaskPlanner generated 2 execution steps with dependency checks.`,
        }),
      });

      // 4. Simulate Execution step
      await new Promise((r) => setTimeout(r, 700));
      await fetch('/api/events', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          event_type: 'EXECUTING',
          message: isLocal ? 'Executing on-device via C++ Core (Phi-4-mini)' : 'Executing via Cloud Adapter (OpenAI / Gemini)',
          provider: isLocal ? 'local_cpp' : 'openai_cloud',
          latency_ms: isLocal ? 24 : 320,
          tokens_used: 120,
        }),
      });

      // 5. Simulate Task Completion
      await new Promise((r) => setTimeout(r, 400));
      await fetch('/api/events', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          event_type: 'COMPLETED',
          message: `Task executed successfully. Output captured and added to working memory.`,
          latency_ms: isLocal ? 24 : 320,
        }),
      });
    } catch (err) {
      console.error('Dispatch error:', err);
    } finally {
      setIsDispatching(false);
    }
  };

  const handleClearEvents = () => {
    setEvents([]);
  };

  return (
    <div className="min-h-screen flex flex-col bg-[var(--bg-surface)] text-[var(--text-primary)] transition-colors">
      <Header
        isConnected={isConnected}
        theme={theme}
        onToggleTheme={toggleTheme}
        onOpenGetApp={() => setIsGetAppOpen(true)}
      />

      <main className="flex-1 max-w-7xl w-full mx-auto p-4 flex flex-col gap-4">
        {/* Quick Action Download Banner */}
        <div className="p-3.5 px-4 rounded-lg border border-[var(--accent-cobalt)]/30 bg-gradient-to-r from-[var(--bg-card)] via-[var(--accent-cobalt-subtle)]/30 to-[var(--bg-card)] flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3 shadow-sm">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 rounded-lg bg-[#14161c] border border-[var(--accent-cobalt)]/40 p-1 flex items-center justify-center flex-shrink-0 shadow-sm">
              <img src="/icon.png" alt="OmniAgent" className="w-full h-full object-contain rounded-md" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <span className="text-xs font-bold text-[var(--text-primary)]">OmniAgent v0.2.1 Native Applications Released</span>
                <span className="px-1.5 py-0.2 text-[10px] font-mono font-semibold rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">NEW</span>
              </div>
              <p className="text-[11px] text-[var(--text-secondary)]">Run the private on-device Voice Assistant on Linux, Windows, or Android.</p>
            </div>
          </div>

          <div className="flex items-center gap-2 w-full sm:w-auto">
            <button
              onClick={() => setIsGetAppOpen(true)}
              className="flex-1 sm:flex-initial flex items-center justify-center gap-1.5 px-3 py-1.5 rounded-md bg-[var(--accent-cobalt)] hover:bg-[var(--accent-cobalt)]/90 text-white font-medium text-xs shadow-sm transition-all"
            >
              <Monitor className="w-3.5 h-3.5" />
              <span>Desktop (Linux/Win)</span>
            </button>
            <button
              onClick={() => setIsGetAppOpen(true)}
              className="flex-1 sm:flex-initial flex items-center justify-center gap-1.5 px-3 py-1.5 rounded-md bg-emerald-600 hover:bg-emerald-500 text-white font-medium text-xs shadow-sm transition-all"
            >
              <Smartphone className="w-3.5 h-3.5" />
              <span>Android APK</span>
            </button>
          </div>
        </div>

        <MetricsGrid
          localRatio={metrics.localTasksRatio}
          cloudRatio={metrics.cloudTasksRatio}
          avgLatencyMs={metrics.avgLatencyMs}
          costSavedUsd={metrics.costSavedUsd}
        />

        <div className="grid grid-cols-1 lg:grid-cols-12 gap-4 flex-1">
          <div className="lg:col-span-5 flex flex-col gap-4">
            <AgentWorkbench onDispatchTask={handleDispatchTask} isDispatching={isDispatching} />
          </div>

          <div className="lg:col-span-7 flex flex-col min-h-[450px]">
            <ReasoningStream events={events} onClearEvents={handleClearEvents} />
          </div>
        </div>

        <ArchitecturePanel />
      </main>

      <GetAppModal isOpen={isGetAppOpen} onClose={() => setIsGetAppOpen(false)} />
    </div>
  );
}
