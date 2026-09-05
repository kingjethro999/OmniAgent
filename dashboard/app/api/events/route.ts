import { NextResponse } from "next/server";
import { getEvents, addEvent, AgentEvent } from "@/lib/eventsStore";

export async function GET() {
  return NextResponse.json({
    status: "ok",
    events: getEvents(),
  });
}

export async function POST(req: Request) {
  try {
    const body = await req.json();
    const event: AgentEvent = {
      id: body.id || Math.random().toString(36).substring(2, 9),
      timestamp: body.timestamp || Date.now() / 1000,
      event_type: body.event_type || "SYSTEM",
      message: body.message || "",
      data: body.data || {},
      routing_decision: body.routing_decision,
      complexity_score: body.complexity_score,
      step_number: body.step_number,
      total_steps: body.total_steps,
      tokens_used: body.tokens_used,
      latency_ms: body.latency_ms,
      provider: body.provider,
    };

    addEvent(event);

    return NextResponse.json({ status: "ok", event });
  } catch (err: any) {
    return NextResponse.json({ error: err.message }, { status: 400 });
  }
}
