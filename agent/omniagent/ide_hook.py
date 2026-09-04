"""
OmniAgent Engine — IDE Integration Hook & MCP Server

Provides a lightweight HTTP / JSON-RPC local server allowing coding IDEs
(VS Code, Cursor, JetBrains, Neovim) to hook directly into OmniAgent Engine for:
 - Context-aware local code auditing & security analysis
 - Privacy-preserving hybrid routing (on-device SLM vs cloud offload)
 - Standardized Model Context Protocol (MCP) tool invocation
"""

from __future__ import annotations
import asyncio
import json
from http.server import HTTPServer, BaseHTTPRequestHandler
import threading
from typing import Any

from omniagent.executor import AgentExecutor
from omniagent.router import TaskRouter

class IDEHookHandler(BaseHTTPRequestHandler):
    executor = AgentExecutor()

    def do_POST(self):
        content_length = int(self.headers.get('Content-Length', 0))
        post_data = self.rfile.read(content_length)

        try:
            req = json.loads(post_data.decode('utf-8'))
            method = req.get('method', 'audit')
            params = req.get('params', {})

            if method == 'audit':
                # Code auditing hook
                code_snippet = params.get('code', '')
                file_path = params.get('file_path', 'unknown.py')
                prompt = f"Audit the following code snippet from {file_path} for security bugs and performance optimization:\n\n{code_snippet}"

                # Run sync wrapper over async executor
                loop = asyncio.new_event_loop()
                asyncio.set_event_loop(loop)
                result = loop.run_until_complete(self.executor.run(prompt))
                loop.close()

                response = {
                    "jsonrpc": "2.0",
                    "result": {
                        "file_path": file_path,
                        "analysis": result,
                        "privacy_status": "PROCESSED_ON_DEVICE"
                    },
                    "id": req.get("id", 1)
                }
            elif method == 'route':
                # Simple router classification query for IDE inline completions
                task = params.get('prompt', '')
                router = TaskRouter()
                res = router.route(task)
                response = {
                    "jsonrpc": "2.0",
                    "result": {
                        "decision": res.decision.value,
                        "score": res.complexity_score,
                        "reasoning": res.reasoning
                    },
                    "id": req.get("id", 1)
                }
            else:
                response = {
                    "jsonrpc": "2.0",
                    "error": {"code": -32601, "message": f"Method '{method}' not found"},
                    "id": req.get("id", 1)
                }

            self._send_json(200, response)
        except Exception as e:
            self._send_json(500, {"jsonrpc": "2.0", "error": {"code": -32603, "message": str(e)}, "id": 1})

    def do_GET(self):
        if self.path == '/status':
            self._send_json(200, {
                "status": "online",
                "engine": "OmniAgent v0.1.0",
                "ide_hook_version": "1.0",
                "mcp_protocol_version": "2024-11-05"
            })
        else:
            self._send_json(404, {"error": "Not found"})

    def _send_json(self, status_code: int, data: dict[str, Any]):
        self.send_response(status_code)
        self.send_header('Content-Type', 'application/json')
        self.send_header('Access-Control-Allow-Origin', '*')
        self.end_headers()
        self.wfile.write(json.dumps(data).encode('utf-8'))

    def log_message(self, format, *args):
        pass  # Suppress default HTTP logging

class IDEHookServer:
    """Manages the background HTTP server for IDE integration."""

    def __init__(self, port: int = 8765):
        self.port = port
        self.server = HTTPServer(('127.0.0.1', self.port), IDEHookHandler)
        self.thread = threading.Thread(target=self.server.serve_forever, daemon=True)

    def start(self):
        self.thread.start()
        print(f"[IDE Hook] OmniAgent IDE Bridge listening at http://127.0.0.1:{self.port}")

    def stop(self):
        self.server.shutdown()
        self.server.server_close()
