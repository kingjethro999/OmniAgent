"""
OmniAgent Engine — CLI Entrypoint
"""

from __future__ import annotations
import asyncio, sys, argparse
from rich.console import Console
from rich.panel import Panel
from rich.prompt import Prompt

from omniagent.executor import AgentExecutor
from omniagent.events import event_bus, AgentEvent, EventType

console = Console()

def event_logger(event: AgentEvent):
    """Log events to terminal with Rich styling."""
    color_map = {
        EventType.THINKING: "cyan",
        EventType.ROUTING: "yellow",
        EventType.PLANNING: "magenta",
        EventType.EXECUTING: "blue",
        EventType.COMPLETED: "green",
        EventType.ERROR: "red",
    }
    color = color_map.get(event.event_type, "white")
    console.print(f"[{color}][{event.event_type.value}][/{color}] {event.message}")

async def main_async(prompt: str | None = None):
    event_bus.subscribe(event_logger)
    executor = AgentExecutor()

    console.print(Panel.fit(
        "[bold cyan]OmniAgent Engine v0.1.0[/bold cyan]\n"
        "[dim]Hybrid Local/Cloud Edge Agent Framework[/dim]",
        border_style="cyan"
    ))

    if prompt:
        res = await executor.run(prompt)
        console.print(Panel(res, title="[bold green]Output[/bold green]", border_style="green"))
        return

    # Interactive REPL
    while True:
        try:
            user_input = Prompt.ask("\n[bold cyan]OmniAgent[/bold cyan]")
            if user_input.lower() in ("exit", "quit", "q"):
                break
            if not user_input.strip():
                continue

            res = await executor.run(user_input)
            console.print(Panel(res, title="[bold green]Response[/bold green]", border_style="green"))
        except (KeyboardInterrupt, EOFError):
            break

def main():
    parser = argparse.ArgumentParser(description="OmniAgent Engine CLI")
    parser.add_argument("prompt", nargs="?", help="Optional task prompt to run immediately")
    parser.add_argument(
        "--ide-hook",
        "--server",
        action="store_true",
        help="Start the OmniAgent IDE Hook & MCP server on localhost",
    )
    parser.add_argument(
        "--port",
        "-p",
        type=int,
        default=8765,
        help="Port for the IDE Hook / MCP server (default: 8765)",
    )
    args = parser.parse_args()

    if args.ide_hook:
        from omniagent.ide_hook import run_standalone
        run_standalone(port=args.port)
        return

    asyncio.run(main_async(args.prompt))

if __name__ == "__main__":
    main()
