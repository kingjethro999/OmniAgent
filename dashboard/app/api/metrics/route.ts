import { NextResponse } from 'next/server';

export async function GET() {
  return NextResponse.json({
    metrics: {
      localTasksRatio: 84,
      cloudTasksRatio: 16,
      avgLatencyMs: 38,
      costSavedUsd: 14.85,
      activeRuntimes: ['C++ Core', 'C# Desktop', 'Java Mobile', 'Python SDK']
    }
  });
}
