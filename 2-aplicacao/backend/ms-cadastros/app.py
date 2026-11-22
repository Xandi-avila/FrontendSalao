import base64, json, os, time, uuid
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import urlparse
from telemetry import TelemetryStore

SERVICE_NAME = "ms-cadastros"
PORT = int(os.environ.get("PORT", "8001"))
telemetry = TelemetryStore(SERVICE_NAME, os.environ.get("LOG_FILE"))

CAT_ID = "11111111-1111-1111-1111-111111111111"
SERV1_ID = "22222222-2222-2222-2222-222222222221"
SERV2_ID = "22222222-2222-2222-2222-222222222222"
FUNC1_ID = "33333333-3333-3333-3333-333333333331"
FUNC2_ID = "33333333-3333-3333-3333-333333333332"
CLI1_ID = "44444444-4444-4444-4444-444444444441"
CLI2_ID = "44444444-4444-4444-4444-444444444442"
PROD1_ID = "55555555-5555-5555-5555-555555555551"

DB = {
    "clientes": [
        {"id": CLI1_ID, "nomeCompleto": "Ana Paula Souza", "whatsApp": "51998123344", "email": "ana.souza@email.com", "instagram": "@anapaula", "profissao": ["Arquiteta"], "dataNascimento": "1992-05-14T00:00:00", "endereco": "Av. Padre Claret, 420"},
        {"id": CLI2_ID, "nomeCompleto": "Mariana Costa", "whatsApp": "51997418890", "email": "mariana.costa@email.com", "instagram": "@maricosta", "profissao": ["Designer"], "dataNascimento": "1995-08-22T00:00:00", "endereco": "Rua Presidente Vargas, 180"},
    ],
    "funcionarios": [
        {"id": FUNC1_ID, "nomeCompleto": "Carla Mendes", "endereco": "Rua Garibaldi, 100", "telefone": "51999112233", "cpf": "12345678901", "dataAdmissao": "2024-02-01T00:00:00", "profissoes": ["Cabeleireira"], "email": "carla@salao.com", "dataNascimento": "1990-03-10T00:00:00", "nivelPermissao": 4, "status": 1, "percentualComissaoProduto": 10.0, "percentualComissaoServico": 35.0},
        {"id": FUNC2_ID, "nomeCompleto": "Lucas Ferreira", "endereco": "Av. Brasil, 55", "telefone": "51998223344", "cpf": "98765432100", "dataAdmissao": "2024-06-15T00:00:00", "profissoes": ["Colorista"], "email": "lucas@salao.com", "dataNascimento": "1993-11-20T00:00:00", "nivelPermissao": 2, "status": 1, "percentualComissaoProduto": 10.0, "percentualComissaoServico": 30.0},
    ],
    "categorias": [
        {"id": CAT_ID, "nome": "Cabelo", "servicoIds": [SERV1_ID, SERV2_ID], "nomesServicosVinculados": ["Corte Feminino + Escova", "Coloração Completa"]},
    ],
    "produtos": [
        {"id": PROD1_ID, "nome": "Shampoo Reconstrutor 500ml", "valor": 89.90, "caminhoImagem": None, "status": 1, "percentualComissao": 10.0},
    ],
    "servicos": [
        {"id": SERV1_ID, "nome": "Corte Feminino + Escova", "duracaoMinutos": 60, "precoMinimo": 140.00, "categoriaId": CAT_ID, "nomeCategoria": "Cabelo", "status": 1, "percentualComissao": 35.0},
        {"id": SERV2_ID, "nome": "Coloração Completa", "duracaoMinutos": 120, "precoMinimo": 260.00, "categoriaId": CAT_ID, "nomeCategoria": "Cabelo", "status": 1, "percentualComissao": 30.0},
    ],
}

def ok(dados, msg="OK"):
    return {"sucesso": True, "mensagem": msg, "erros": [], "dados": dados}

def fail(msg):
    return {"sucesso": False, "mensagem": msg, "erros": [msg], "dados": None}

def make_jwt(email: str) -> str:
    h = base64.urlsafe_b64encode(json.dumps({"alg": "HS256", "typ": "JWT"}).encode()).decode().rstrip("=")
    p = base64.urlsafe_b64encode(json.dumps({"sub": email, "email": email, "name": "Administrador", "role": "Administrador", "nivelPermissao": "Administrador", "exp": int(time.time()) + 86400}).encode()).decode().rstrip("=")
    s = base64.urlsafe_b64encode(b"salao-admin-cloud-signature").decode().rstrip("=")
    return f"{h}.{p}.{s}"

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
        if method == "POST" and path == "/auth/login":
            email = payload.get("email", "")
            senha = payload.get("senha", "")
            if not email or senha == "invalida":
                status, resp = 401, fail("Credenciais inválidas")
            else:
                resp = ok({"token": make_jwt(email), "funcionario": {"id": FUNC1_ID, "nomeCompleto": "Carla Mendes", "email": email, "nivelPermissao": 4}})
        else:
            parts = [p for p in path.split("/") if p]
            res = parts[0] if parts else ""
            if res in DB:
                if method == "GET" and len(parts) == 1:
                    items = DB[res]
                    resp = ok({"pagina": 1, "tamanho": 20, "total": len(items), "itens": items})
                elif method == "GET" and len(parts) == 2:
                    item = next((x for x in DB[res] if str(x["id"]) == parts[1]), None)
                    resp = ok(item) if item else fail("Registro não encontrado")
                    if not item: status = 404
                elif method == "POST" and len(parts) == 1:
                    item = {"id": str(uuid.uuid4()), **payload}
                    DB[res].append(item)
                    status, resp = 201, ok(item, "Criado com sucesso")
                elif method == "PUT" and len(parts) == 2:
                    idx = next((i for i, x in enumerate(DB[res]) if str(x["id"]) == parts[1]), None)
                    if idx is not None:
                        DB[res][idx] = {"id": parts[1], **payload}
                        resp = ok(DB[res][idx], "Atualizado")
                    else:
                        status, resp = 404, fail("Registro não encontrado")
                elif method == "DELETE" and len(parts) == 2:
                    DB[res] = [x for x in DB[res] if str(x["id"]) != parts[1]]
                    resp = ok(None, "Removido")
            else:
                status, resp = 404, fail("Recurso desconhecido")

        telemetry.record_http(method, path, status, (time.perf_counter() - t0) * 1000.0, corr)
        self._send(status, resp)

    def do_GET(self): self._handle("GET")
    def do_POST(self): self._handle("POST")
    def do_PUT(self): self._handle("PUT")
    def do_DELETE(self): self._handle("DELETE")

if __name__ == "__main__":
    telemetry.log("INFO", f"Iniciando {SERVICE_NAME} na porta {PORT}")
    ThreadingHTTPServer(("0.0.0.0", PORT), Handler).serve_forever()

