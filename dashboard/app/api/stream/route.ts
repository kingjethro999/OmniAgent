import { subscribeToEvents, getEvents, AgentEvent } from "@/lib/eventsStore";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

export async function GET() {
  const encoder = new TextEncoder();

  const customStream = new ReadableStream({
    start(controller) {
      // Send initial state
      const initialEvents = getEvents();
      const initData = `data: ${JSON.stringify({ type: "INIT", events: initialEvents })}\n\n`;
      controller.enqueue(encoder.encode(initData));

      // Subscribe to real-time events
      const unsubscribe = subscribeToEvents((event: AgentEvent) => {
        try {
          const chunk = `data: ${JSON.stringify({ type: "EVENT", event })}\n\n`;
          controller.enqueue(encoder.encode(chunk));
        } catch (e) {
          // Stream closed
        }
      });

      // Keep connection alive with pings every 15s
      const interval = setInterval(() => {
        try {
          controller.enqueue(encoder.encode(": ping\n\n"));
        } catch (e) {
          clearInterval(interval);
        }
      }, 15000);

      // Cleanup when connection closes
      return () => {
        clearInterval(interval);
        unsubscribe();
      };
    },
  });

  return new Response(customStream, {
    headers: {
      "Content-Type": "text/event-stream",
      "Cache-Control": "no-cache, no-transform",
      Connection: "keep-alive",
    },
  });
}
