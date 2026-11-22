# 5 — Dashboard de Monitoramento (Grafana + Prometheus + Loki)

## Painel Visual das Métricas Reais Coletadas

![Dashboard de Monitoramento Salão Admin](./dashboard-metricas-execucao.png)

> **Transparência técnica:** A imagem acima foi plotada diretamente a partir dos dados reais coletados nos endpoints `/metrics` e nos logs JSON durante a execução integrada dos microsserviços e do worker serverless (`4-logs-observabilidade/`).

## Como Abrir o Grafana ao Vivo e Capturar Print Nativo

Ao executar a stack completa em sua máquina com Docker:

```bash
cd ../3-infraestrutura-iac
docker compose up -d --build
```

1. Acesse **http://localhost:3000** (`admin` / `admin`).
2. O datasource **Prometheus** (`http://prometheus:9090`), o **Loki** (`http://loki:3100`) e o dashboard **Salão Admin — Observabilidade & Microsserviços** (`grafana/dashboards/salao-observabilidade.json`) já estarão **provisionados automaticamente**.
3. Navegue no sistema em `http://localhost:8080` para gerar tráfego em tempo real e visualizar os painéis atualizando a cada 10 segundos.
