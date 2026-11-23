"""
Função de notificações do Salão Admin.
- Na AWS: roda como Lambda, acionada pelo SQS (entrada lambda_handler).
- No Docker: roda como serviço HTTP na porta 8004, com /events, /health e /metrics.
"""
import json
import os
import time
import uuid
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import urlparse
from telemetry import TelemetryStore

SERVICE_NAME = "serverless-notificacoes"
PORT = int(os.environ.get("PORT", "8004"))
telemetry = TelemetryStore(SERVICE_NAME, os.environ.get("EVENT_LOG_FILE"))
PROCESSED_IDS: set[str] = set()


def process_single_event(envelope: dict) -> dict:
    """Processa evento com idempotência e retorna resultado de auditoria."""
    t0 = time.perf_counter()
    event_id = envelope.get("event_id", str(uuid.uuid4()))
    event_type = envelope.get("event_type", "desconhecido")
    corr_id = envelope.get("correlation_id", "n/a")
    payload = envelope.get("payload", {})

    if event_id in PROCESSED_IDS:
        telemetry.events_processed[(event_type, "duplicado")] = (
            telemetry.events_processed.get((event_type, "duplicado"), 0) + 1
        )
        telemetry.log("WARN", "Evento duplicado ignorado (idempotência)", correlation_id=corr_id, event_id=event_id, event_type=event_type)
        return {"status": "ignored_duplicate", "event_id": event_id}

    # Simula ação assíncrona de notificação (WhatsApp/E-mail) e registro de auditoria
    time.sleep(0.015)
    action_map = {
        "agendamento.criado": "Lembrete de confirmação enfileirado para o cliente",
        "agendamento.cancelado": "Aviso de cancelamento e liberação de janela processados",
        "venda.concluida": "Comprovante digital emitido e baixa analítica registrada",
    }
    action_desc = action_map.get(event_type, "Evento genérico auditado")
    PROCESSED_IDS.add(event_id)
    latency_ms = round((time.perf_counter() - t0) * 1000.0, 2)

    telemetry.events_processed[(event_type, "sucesso")] = (
        telemetry.events_processed.get((event_type, "sucesso"), 0) + 1
    )
    telemetry.log(
        "INFO",
        action_desc,
        correlation_id=corr_id,
        event_id=event_id,
        event_type=event_type,
        processing_ms=latency_ms,
        entity_id=payload.get("id"),
    )
    return {"status": "processed", "event_id": event_id, "action": action_desc, "processing_ms": latency_ms}


def lambda_handler(event, context=None):
    """Entrypoint padrão AWS Lambda (trigger Amazon SQS com batchItemFailures)."""
    batch_item_failures = []
    results = []
    for record in event.get("Records", []):
        try:
            body = json.loads(record["body"]) if isinstance(record.get("body"), str) else record.get("body", {})
            results.append(process_single_event(body))
        except Exception as exc:
            telemetry.log("ERROR", f"Falha ao processar mensagem SQS: {exc}", message_id=record.get("messageId"))
            batch_item_failures.append({"itemIdentifier": record.get("messageId")})
    return {"batchItemFailures": batch_item_failures, "processed": len(results)}


class LocalWorkerHandler(BaseHTTPRequestHandler):
    def log_message(self, format, *args):
        return

    def _send(self, status: int, body, content_type="application/json"):
        raw = body.encode("utf-8") if isinstance(body, str) else json.dumps(body, ensure_ascii=False).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", content_type)
        self.send_header("Content-Length", str(len(raw)))
        self.end_headers()
        self.wfile.write(raw)

    def do_GET(self):
        path = urlparse(self.path).path.rstrip("/") or "/"
        if path in ("/health", "/ready"):
            self._send(200, {"status": "ok", "service": SERVICE_NAME})
        elif path == "/metrics":
            self._send(200, telemetry.render_prometheus(), "text/plain; version=0.0.4")
        else:
            self._send(404, {"erro": "Rota não encontrada"})

    def do_POST(self):
        path = urlparse(self.path).path.rstrip("/") or "/"
        if path == "/events":
            length = int(self.headers.get("Content-Length", "0"))
            envelope = json.loads(self.rfile.read(length).decode("utf-8")) if length > 0 else {}
            res = process_single_event(envelope)
            self._send(202, res)
        else:
            self._send(404, {"erro": "Rota não encontrada"})


if __name__ == "__main__":
    telemetry.log("INFO", f"Iniciando {SERVICE_NAME} (modo worker local) na porta {PORT}")
    ThreadingHTTPServer(("0.0.0.0", PORT), LocalWorkerHandler).serve_forever()
