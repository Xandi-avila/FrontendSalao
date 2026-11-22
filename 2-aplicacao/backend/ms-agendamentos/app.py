import json
import os
import time
import uuid
import uuid
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import urlparse
from telemetry import TelemetryStore, dispatch_event

SERVICE_NAME = "ms-agendamentos"
PORT = int(os.environ.get("PORT", "8002"))
telemetry = TelemetryStore(SERVICE_NAME, os.environ.get("LOG_FILE"))

AGENDAMENTOS = [
    {
        "id": 101,
        "clienteId": 1,
        "clienteNome": "Ana Paula Souza",
        "funcionarioId": 1,
        "servicoId": 1,
        "dataHora": "2026-11-25T14:00:00-03:00",
        "status": "Confirmado",
    }
]

JANELAS = {
    1: [
        {"id": 1, "diaSemana": "Segunda", "inicio": "09:00", "fim": "18:00"},
        {"id": 2, "diaSemana": "Quarta", "inicio": "09:00", "fim": "18:00"},
    ]
}


class Handler(BaseHTTPRequestHandler):
    def log_message(self, format, *args):
        return

    def _send(self, status: int, body, content_type="application/json"):
        raw = body.encode("utf-8") if isinstance(body, str) else json.dumps(body, ensure_ascii=False).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", content_type)
        self.send_header("Content-Length", str(len(raw)))
        self.end_headers()
        self.wfile.write(raw)

    def _handle(self, method: str):
        t0 = time.perf_counter()
        corr = self.headers.get("X-Correlation-ID", str(uuid.uuid4()))
        path = urlparse(self.path).path.rstrip("/") or "/"
        length = int(self.headers.get("Content-Length", "0"))
        payload = json.loads(self.rfile.read(length).decode("utf-8")) if length > 0 else {}

        if path in ("/health", "/ready"):
            self._send(200, {"status": "ok", "service": SERVICE_NAME})
            return
        if path == "/metrics":
            self._send(200, telemetry.render_prometheus(), "text/plain; version=0.0.4")
            return

        status, resp = 200, {}
        parts = [p for p in path.split("/") if p]
        if parts == ["agendamentos"] and method == "GET":
            resp = {"itens": AGENDAMENTOS, "totalRegistros": len(AGENDAMENTOS), "pagina": 1, "tamanhoPagina": 20}
        elif parts == ["agendamentos"] and method == "POST":
            new_id = max((x["id"] for x in AGENDAMENTOS), default=100) + 1
            item = {"id": new_id, "status": "Agendado", **payload}
            AGENDAMENTOS.append(item)
            dispatch_event("agendamento.criado", item, corr, telemetry)
            status, resp = 201, item
        elif len(parts) == 3 and parts[0] == "agendamentos" and parts[2] == "cancelar" and method == "PATCH":
            aid = int(parts[1])
            item = next((x for x in AGENDAMENTOS if x["id"] == aid), None)
            if item:
                item["status"] = "Cancelado"
                dispatch_event("agendamento.cancelado", item, corr, telemetry)
                resp = item
            else:
                status, resp = 404, {"erro": "Agendamento não encontrado"}
        elif len(parts) == 3 and parts[0] == "agenda" and parts[1] == "funcionarios" and method == "GET":
            fid = int(parts[2])
            resp = JANELAS.get(fid, [])
        else:
            status, resp = 404, {"erro": "Rota não encontrada"}

        latency_ms = (time.perf_counter() - t0) * 1000.0
        telemetry.record_http(method, path, status, latency_ms, corr)
        self._send(status, resp)

    def do_GET(self): self._handle("GET")
    def do_POST(self): self._handle("POST")
    def do_PUT(self): self._handle("PUT")
    def do_PATCH(self): self._handle("PATCH")
    def do_DELETE(self): self._handle("DELETE")


if __name__ == "__main__":
    telemetry.log("INFO", f"Iniciando {SERVICE_NAME} na porta {PORT}")
    ThreadingHTTPServer(("0.0.0.0", PORT), Handler).serve_forever()
