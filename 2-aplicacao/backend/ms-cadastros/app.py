import base64
import json
import os
import time
import uuid
import uuid
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import urlparse
from telemetry import TelemetryStore

SERVICE_NAME = "ms-cadastros"
PORT = int(os.environ.get("PORT", "8001"))
telemetry = TelemetryStore(SERVICE_NAME, os.environ.get("LOG_FILE"))

DB = {
    "clientes": [
        {"id": 1, "nome": "Ana Paula Souza", "telefone": "(51) 99812-3344", "email": "ana.souza@email.com", "ativo": True},
        {"id": 2, "nome": "Mariana Costa", "telefone": "(51) 99741-8890", "email": "mariana.costa@email.com", "ativo": True},
    ],
    "funcionarios": [
        {"id": 1, "nome": "Carla Mendes", "cargo": "Cabeleireira Sênior", "ativo": True},
        {"id": 2, "nome": "Lucas Ferreira", "cargo": "Colorista", "ativo": True},
    ],
    "categorias": [
        {"id": 1, "nome": "Cabelo", "ativo": True},
        {"id": 2, "nome": "Estética Facial", "ativo": True},
    ],
    "produtos": [
        {"id": 1, "nome": "Shampoo Reconstrutor 500ml", "categoriaId": 1, "preco": 89.90, "estoque": 18},
        {"id": 2, "nome": "Máscara Hidratação Profunda", "categoriaId": 1, "preco": 124.50, "estoque": 12},
    ],
    "servicos": [
        {"id": 1, "nome": "Corte Feminino + Escova", "categoriaId": 1, "preco": 140.00, "duracaoMinutos": 60},
        {"id": 2, "nome": "Coloração Completa", "categoriaId": 1, "preco": 260.00, "duracaoMinutos": 120},
    ],
}


def make_jwt(email: str) -> str:
    header = base64.urlsafe_b64encode(json.dumps({"alg": "HS256", "typ": "JWT"}).encode()).decode().rstrip("=")
    payload = base64.urlsafe_b64encode(
        json.dumps({"sub": email, "role": "Admin", "exp": int(time.time()) + 3600}).encode()
    ).decode().rstrip("=")
    sig = base64.urlsafe_b64encode(b"salao-admin-cloud-signature").decode().rstrip("=")
    return f"{header}.{payload}.{sig}"


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

        status = 200
        resp = {}
        if method == "POST" and path == "/auth/login":
            email = payload.get("email", "")
            senha = payload.get("senha", "")
            if not email or senha == "invalida":
                status = 401
                resp = {"erro": "Credenciais inválidas"}
            else:
                resp = {"token": make_jwt(email), "usuario": email, "perfil": "Admin"}
        else:
            parts = [p for p in path.split("/") if p]
            resource = parts[0] if parts else ""
            if resource in DB:
                if method == "GET" and len(parts) == 1:
                    items = DB[resource]
                    resp = {"itens": items, "totalRegistros": len(items), "pagina": 1, "tamanhoPagina": 20}
                elif method == "GET" and len(parts) == 2 and parts[1].isdigit():
                    item = next((x for x in DB[resource] if x["id"] == int(parts[1])), None)
                    if item:
                        resp = item
                    else:
                        status, resp = 404, {"erro": f"{resource} não encontrado"}
                elif method == "POST" and len(parts) == 1:
                    new_id = max((x["id"] for x in DB[resource]), default=0) + 1
                    item = {"id": new_id, **payload}
                    DB[resource].append(item)
                    status, resp = 201, item
                elif method == "PUT" and len(parts) == 2 and parts[1].isdigit():
                    idx = next((i for i, x in enumerate(DB[resource]) if x["id"] == int(parts[1])), None)
                    if idx is not None:
                        DB[resource][idx] = {"id": int(parts[1]), **payload}
                        resp = DB[resource][idx]
                    else:
                        status, resp = 404, {"erro": "Registro não encontrado"}
                elif method == "DELETE" and len(parts) == 2 and parts[1].isdigit():
                    DB[resource] = [x for x in DB[resource] if x["id"] != int(parts[1])]
                    resp = {"removido": True}
                else:
                    status, resp = 404, {"erro": "Rota não encontrada"}
            else:
                status, resp = 404, {"erro": "Recurso desconhecido"}

        latency_ms = (time.perf_counter() - t0) * 1000.0
        telemetry.record_http(method, path, status, latency_ms, corr)
        self._send(status, resp)

    def do_GET(self): self._handle("GET")
    def do_POST(self): self._handle("POST")
    def do_PUT(self): self._handle("PUT")
    def do_DELETE(self): self._handle("DELETE")


if __name__ == "__main__":
    telemetry.log("INFO", f"Iniciando {SERVICE_NAME} na porta {PORT}")
    ThreadingHTTPServer(("0.0.0.0", PORT), Handler).serve_forever()
