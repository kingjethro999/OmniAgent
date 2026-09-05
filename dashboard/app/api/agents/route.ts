import { NextResponse } from "next/server";

export async function GET() {
  const agents = [
    {
      id: "omni-core-01",
      name: "OmniAgent Core Engine",
      status: "IDLE",
      currentTask: "Awaiting task dispatch...",
      localModel: "Phi-4-mini (1B-3B GGUF / C++ Core)",
      cloudModel: "GPT-4o / Gemini 2.0 Flash",
      architecture: "Hybrid Edge-Cloud (Vulkan / WebGPU)",
      tasksCompleted: 42,
      routingRatio: { local: 84, cloud: 16 },
    },
  ];

  return NextResponse.json({ agents });
}
