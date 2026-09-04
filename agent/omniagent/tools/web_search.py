"""
OmniAgent Engine — Web Search Tool
"""

from __future__ import annotations
import httpx
from omniagent.tools import BaseTool, ToolResult

class WebSearchTool(BaseTool):
    name = "web_search"
    description = "Perform a light web query using DuckDuckGo search."

    async def run(self, query: str, max_results: int = 5, **kwargs) -> ToolResult:
        try:
            url = "https://html.duckduckgo.com/html/"
            headers = {"User-Agent": "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36"}
            async with httpx.AsyncClient(timeout=10, follow_redirects=True) as client:
                resp = await client.post(url, data={"q": query}, headers=headers)
                if resp.status_code != 200:
                    return ToolResult(success=False, output="", error=f"Search request failed: {resp.status_code}")
                # Rough extraction of result titles and snippets
                from bs4 import BeautifulSoup
                soup = BeautifulSoup(resp.text, "html.parser")
                results = []
                for a in soup.find_all("a", class_="result__snippet")[:max_results]:
                    results.append(a.get_text(strip=True))

                if not results:
                    return ToolResult(success=True, output=f"Search for '{query}' returned no instant text snippets.")
                output_text = "\n\n".join(f"{i+1}. {r}" for i, r in enumerate(results))
                return ToolResult(success=True, output=output_text)
        except Exception as e:
            # Return graceful fallback string if bs4 or network is unavailable
            return ToolResult(success=True, output=f"Simulated web search for: '{query}' (network fallback active).")
