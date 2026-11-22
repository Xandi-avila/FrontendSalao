import json, os, time, uuid
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import urlparse
from telemetry import TelemetryStore, dispatch_event

SERVICE_NAME = "ms-agendamentos"
PORT = int(os.environ.get("PORT", "8002"))
telemetry = TelemetryStore(SERVICE_NAME, os.environ.get("LOG_FILE"))

AGENDAMENTOS = [
    {"id": "66666666-6666-6666-6666-666666666661", "clienteId": "44444444-4444-4444-4444-444444444441", "funcionarioId": "33333333-3333-3333-3333-333333333331", "servicoIds": ["22222222-2222-2222-2222-222222222221"], "dataHoraInicio": "2026-11-25T14:00:00", "dataHoraFim": "2026-11-25T15:00:00", "status": "CONFIRMADO"}
]

def ok(dados, msg="OK"):
    return {"sucesso": True, "mensagem": msg, "erros": [], "dados": dados}

class Handler(BaseHTTPRequestHandler):
    def log_message(self, format, *args): return
    def _send(self, status, body, content_type="application/json"):
        raw = body.encode("utf-8") if isinstance(body, str) else json.dumps(body, ensure_ascii=False).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", content_type)
        self.send_header("Content-Length", str(len(raw)))
        self.end_headers()
        self.wfile.write(raw)

    def _handle(self, method):
        t0 = time.perf_counter()
        corr = self.headers.get("X-Correlation-ID", str(uuid.uuid4()))
        path = urlparse(self.path).path.rstrip("/") or "/"
        length = int(self.headers.get("Content-Length", "0"))
        payload = json.loads(self.rfile.read(length).decode("utf-8")) if length > 0 else {}

        if path in ("/health", "/ready"):
            self._send(200, {"status": "ok", "service": SERVICE_NAME}); return
        if path == "/metrics":
            self._send(200, telemetry.render_prometheus(), "text/plain; version=0.0.4"); return

        status, resp = 200, ok(None)
        parts = [p for p in path.split("/") if p]
        if parts == ["agendamentos"] and method == "GET":
            resp = ok({"pagina": 1, "tamanho": 20, "total": len(AGENDAMENTOS), "itens": AGENDAMENTOS})
        elif parts == ["agendamentos"] and method == "POST":
            item = {"id": str(uuid.uuid4()), "status": "AGENDADO", **payload}
            AGENDAMENTOS.append(item)
            dispatch_event("agendamento.criado", item, corr, telemetry)
            status, resp = 201, ok(item, "Agendamento criado")
        elif len(parts) == 3 and parts[0] == "agendamentos" and parts[2] == "cancelar" and method == "PATCH":
            item = next((x for x in AGENDAMENTOS if str(x["id"]) == parts[1]), None)
            if item:
                item["status"] = "CANCELADO"
                dispatch_event("agendamento.cancelado", item, corr, telemetry)
                resp = ok(item, "Cancelado")
            else:
                status, resp = 404, {"sucesso": False, "mensagem": "Não encontrado", "erros": ["Não encontrado"], "dados": None}
        elif len(parts) >= 2 and parts[0] == "agenda":
            resp = ok([])
        else:
            status, resp = 404, {"sucesso": False, "mensagem": "Rota não encontrada", "erros": [], "dados": None}

        telemetry.record_http(method, path, status, (time.perf_counter() - t0) * 1000.0, corr)
        self._send(status, resp)

    def do_GET(self): self._handle("GET")
    def do_POST(self): self._handle("POST")
    def do_PUT(self): self._handle("PUT")
    def do_PATCH(self): self._handle("PATCH")
    def do_DELETE(self): self._handle("DELETE")

if __name__ == "__main__":
    telemetry.log("INFO", f"Iniciando {SERVICE_NAME} na porta {PORT}")
    ThreadingHTTPServer(("0.0.0.0", PORT), Handler).serve_forever()
