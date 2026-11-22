import json, os, time, uuid
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import urlparse
from telemetry import TelemetryStore, dispatch_event

SERVICE_NAME = "ms-vendas-analise"
PORT = int(os.environ.get("PORT", "8003"))
telemetry = TelemetryStore(SERVICE_NAME, os.environ.get("LOG_FILE"))

VENDAS = [
    {"id": "77777777-7777-7777-7777-777777777771", "numero": 1, "dataHora": "2026-11-22T15:10:00", "total": 229.90, "funcionarioId": "33333333-3333-3333-3333-333333333331", "clienteId": "44444444-4444-4444-4444-444444444441", "itens": []}
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
        if path == "/vendas" and method == "GET":
            resp = ok({"pagina": 1, "tamanho": 20, "total": len(VENDAS), "itens": VENDAS})
        elif path == "/vendas" and method == "POST":
            item = {"id": str(uuid.uuid4()), "numero": len(VENDAS) + 1, "dataHora": "2026-11-22T18:00:00", "total": 140.0, **payload}
            VENDAS.append(item)
            dispatch_event("venda.concluida", item, corr, telemetry)
            status, resp = 201, ok(item, "Venda registrada")
        elif path == "/analise/faturamento":
            total = sum(v.get("total", 0.0) for v in VENDAS)
            resp = ok({"faturamentoTotal": round(total, 2), "quantidadeVendas": len(VENDAS), "ticketMedio": round(total / max(len(VENDAS), 1), 2)})
        elif path == "/analise/funcionarios":
            resp = ok([{"funcionarioNome": "Carla Mendes", "quantidadeAtendimentos": 18, "valorTotal": 2520.00}])
        elif path == "/analise/servicos":
            resp = ok([{"servicoNome": "Corte Feminino + Escova", "quantidade": 18, "valorTotal": 2520.00}])
        else:
            status, resp = 404, {"sucesso": False, "mensagem": "Rota não encontrada", "erros": [], "dados": None}

        telemetry.record_http(method, path, status, (time.perf_counter() - t0) * 1000.0, corr)
        self._send(status, resp)

    def do_GET(self): self._handle("GET")
    def do_POST(self): self._handle("POST")

if __name__ == "__main__":
    telemetry.log("INFO", f"Iniciando {SERVICE_NAME} na porta {PORT}")
    ThreadingHTTPServer(("0.0.0.0", PORT), Handler).serve_forever()
