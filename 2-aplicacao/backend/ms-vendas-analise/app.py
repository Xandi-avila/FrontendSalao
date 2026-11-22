import json
import os
import time
import uuid
import uuid
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import urlparse
from telemetry import TelemetryStore, dispatch_event

SERVICE_NAME = "ms-vendas-analise"
PORT = int(os.environ.get("PORT", "8003"))
telemetry = TelemetryStore(SERVICE_NAME, os.environ.get("LOG_FILE"))

VENDAS = [
    {"id": 501, "clienteId": 1, "valorTotal": 229.90, "formaPagamento": "PIX", "data": "2026-11-22T15:10:00-03:00"},
    {"id": 502, "clienteId": 2, "valorTotal": 260.00, "formaPagamento": "Cartão Crédito", "data": "2026-11-22T16:40:00-03:00"},
]


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
        if path == "/vendas" and method == "GET":
            resp = {"itens": VENDAS, "totalRegistros": len(VENDAS), "pagina": 1, "tamanhoPagina": 20}
        elif path == "/vendas" and method == "POST":
            new_id = max((x["id"] for x in VENDAS), default=500) + 1
            item = {"id": new_id, **payload}
            VENDAS.append(item)
            dispatch_event("venda.concluida", item, corr, telemetry)
            status, resp = 201, item
        elif path == "/analise/faturamento" and method == "GET":
            total = sum(v.get("valorTotal", 0.0) for v in VENDAS)
            resp = {"faturamentoTotal": round(total, 2), "quantidadeVendas": len(VENDAS), "ticketMedio": round(total / max(len(VENDAS), 1), 2)}
        elif path == "/analise/funcionarios" and method == "GET":
            resp = [{"funcionario": "Carla Mendes", "atendimentos": 18, "receita": 2520.00}, {"funcionario": "Lucas Ferreira", "atendimentos": 11, "receita": 2860.00}]
        elif path == "/analise/servicos" and method == "GET":
            resp = [{"servico": "Corte Feminino + Escova", "quantidade": 18}, {"servico": "Coloração Completa", "quantidade": 11}]
        else:
            status, resp = 404, {"erro": "Rota não encontrada"}

        latency_ms = (time.perf_counter() - t0) * 1000.0
        telemetry.record_http(method, path, status, latency_ms, corr)
        self._send(status, resp)

    def do_GET(self): self._handle("GET")
    def do_POST(self): self._handle("POST")


if __name__ == "__main__":
    telemetry.log("INFO", f"Iniciando {SERVICE_NAME} na porta {PORT}")
    ThreadingHTTPServer(("0.0.0.0", PORT), Handler).serve_forever()
