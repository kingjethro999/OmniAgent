import { NextResponse } from 'next/server';

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

export async function GET() {
  return NextResponse.json({
    status: 'ok',
    events: globalStore.omniEvents || []
  });
}

export async function POST(req: Request) {
  try {
    const body = await req.json();
    const event: AgentEvent = {
      id: body.id || Math.random().toString(36).substring(2, 9),
      timestamp: body.timestamp || Date.now() / 1000,
      event_type: body.event_type || 'SYSTEM',
      message: body.message || '',
      data: body.data || {},
      routing_decision: body.routing_decision,
      complexity_score: body.complexity_score,
      step_number: body.step_number,
      total_steps: body.total_steps,
      tokens_used: body.tokens_used,
      latency_ms: body.latency_ms,
      provider: body.provider,
    };

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

    return NextResponse.json({ status: 'ok', event });
  } catch (err: any) {
    return NextResponse.json({ error: err.message }, { status: 400 });
  }
}
