export interface AgentEvent {
  id: string;
  timestamp: number;
  event_type: 'THINKING' | 'PLANNING' | 'ROUTING' | 'EXECUTING' | 'TOOL_CALL' | 'TOOL_RESULT' | 'COMPLETED' | 'ERROR' | 'SYSTEM';
  agent_id?: string;
  message: string;
  data?: Record<string, any>;
  routing_decision?: 'LOCAL' | 'CLOUD';
  complexity_score?: number;
  step_number?: number;
  total_steps?: number;
  tokens_used?: number;
  latency_ms?: number;
  provider?: string;
}

// In-memory event log store for the Next.js process
const globalStore = global as unknown as {
  omniEvents?: AgentEvent[];
  omniSubscribers?: Set<(event: AgentEvent) => void>;
};

if (!globalStore.omniEvents) {
  globalStore.omniEvents = [
    {
      id: 'sys-init',
      timestamp: Date.now() / 1000,
      event_type: 'SYSTEM',
      message: 'OmniAgent Engine Next.js Dashboard initialized.',
      data: { runtime: 'Next.js App Router' }
    }
  ];
}

if (!globalStore.omniSubscribers) {
  globalStore.omniSubscribers = new Set();
}

export function getEvents(): AgentEvent[] {
  return globalStore.omniEvents || [];
}

export function subscribeToEvents(fn: (event: AgentEvent) => void) {
  globalStore.omniSubscribers?.add(fn);
  return () => {
    globalStore.omniSubscribers?.delete(fn);
  };
}

export function addEvent(event: AgentEvent) {
  globalStore.omniEvents?.unshift(event);
  if ((globalStore.omniEvents?.length || 0) > 300) {
    globalStore.omniEvents?.pop();
  }

  // Broadcast to SSE clients
  globalStore.omniSubscribers?.forEach((callback) => {
    try {
      callback(event);
    } catch (err) {
      console.error('Subscriber notify error:', err);
    }
  });
}
