import json
import os
import time
import re
import uuid
import urllib.request
from datetime import datetime, timezone
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import urlparse

# IDs em GUID viram "<id>" nas métricas, para não criar uma série nova por registro
ID_GUID = re.compile(r"^[0-9a-fA-F-]{36}$")


class TelemetryStore:
    def __init__(self, service_name: str, log_file: str | None = None):
        self.service_name = service_name
        self.log_file = log_file
        self.requests_total: dict[tuple[str, str, int], int] = {}
        self.latency_sum: dict[str, float] = {}
        self.latency_count: dict[str, int] = {}
        self.events_published: dict[str, int] = {}
        self.events_processed: dict[tuple[str, str], int] = {}

    def log(self, level: str, message: str, **fields):
        entry = {
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "level": level,
            "service": self.service_name,
            "message": message,
            **fields,
        }
        line = json.dumps(entry, ensure_ascii=False)
        print(line, flush=True)
        if self.log_file:
            os.makedirs(os.path.dirname(self.log_file), exist_ok=True)
            with open(self.log_file, "a", encoding="utf-8") as f:
                f.write(line + "\n")

    def record_http(self, method: str, path: str, status: int, latency_ms: float, corr_id: str):
        norm_path = "/" + "/".join(
            "<id>" if seg.isdigit() or ID_GUID.match(seg) else seg for seg in path.strip("/").split("/") if seg
        )
        key = (method, norm_path, status)
        self.requests_total[key] = self.requests_total.get(key, 0) + 1
        self.latency_sum[norm_path] = self.latency_sum.get(norm_path, 0.0) + latency_ms
        self.latency_count[norm_path] = self.latency_count.get(norm_path, 0) + 1
        level = "ERROR" if status >= 500 else ("WARN" if status >= 400 else "INFO")
        self.log(
            level,
            f"HTTP {method} {path} -> {status}",
            correlation_id=corr_id,
            method=method,
            path=path,
            status_code=status,
            latency_ms=round(latency_ms, 2),
        )

    def render_prometheus(self) -> str:
        lines = [
            f"# HELP http_requests_total Total de requisições HTTP processadas",
            f"# TYPE http_requests_total counter",
        ]
        for (method, path, status), count in sorted(self.requests_total.items()):
            lines.append(
                f'http_requests_total{{service="{self.service_name}",method="{method}",path="{path}",status="{status}"}} {count}'
            )
        lines.append("# HELP http_request_duration_ms_sum Soma das latências HTTP em ms")
        lines.append("# TYPE http_request_duration_ms_sum counter")
        for path, total in sorted(self.latency_sum.items()):
            lines.append(
                f'http_request_duration_ms_sum{{service="{self.service_name}",path="{path}"}} {total:.2f}'
            )
            lines.append(
                f'http_request_duration_ms_count{{service="{self.service_name}",path="{path}"}} {self.latency_count[path]}'
            )
        if self.events_published:
            lines.append("# HELP messaging_events_published_total Eventos publicados na fila")
            lines.append("# TYPE messaging_events_published_total counter")
            for ev, cnt in sorted(self.events_published.items()):
                lines.append(
                    f'messaging_events_published_total{{service="{self.service_name}",event_type="{ev}"}} {cnt}'
                )
        if self.events_processed:
            lines.append("# HELP serverless_events_processed_total Eventos consumidos pela função serverless")
            lines.append("# TYPE serverless_events_processed_total counter")
            for (ev, st), cnt in sorted(self.events_processed.items()):
                lines.append(
                    f'serverless_events_processed_total{{service="{self.service_name}",event_type="{ev}",status="{st}"}} {cnt}'
                )
        return "\n".join(lines) + "\n"


def dispatch_event(event_type: str, payload: dict, correlation_id: str, telemetry: TelemetryStore):
    worker_url = os.environ.get("SERVERLESS_WORKER_URL", "http://serverless-notificacoes:8004/events")
    envelope = {
        "event_id": str(uuid.uuid4()),
        "event_type": event_type,
        "timestamp": datetime.now(timezone.utc).isoformat(),
        "correlation_id": correlation_id,
        "source": telemetry.service_name,
        "payload": payload,
    }
    telemetry.events_published[event_type] = telemetry.events_published.get(event_type, 0) + 1
    telemetry.log(
        "INFO",
        f"Evento publicado na fila de mensageria: {event_type}",
        correlation_id=correlation_id,
        event_id=envelope["event_id"],
        event_type=event_type,
        queue="salao-eventos-queue",
    )
    try:
        req = urllib.request.Request(
            worker_url,
            data=json.dumps(envelope).encode("utf-8"),
            headers={"Content-Type": "application/json", "X-Correlation-ID": correlation_id},
            method="POST",
        )
        with urllib.request.urlopen(req, timeout=2.5) as resp:
            resp.read()
    except Exception as exc:
        telemetry.log(
            "WARN",
            f"Falha ao entregar o evento para a função de notificações: {exc}",
            correlation_id=correlation_id,
            event_type=event_type,
        )
