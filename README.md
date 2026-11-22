# Salão Admin — Arquitetura em Nuvem (Microsserviços, Serverless & Observabilidade)

Repositório oficial do trabalho da disciplina de **Arquitetura em Nuvem**, estruturando a solução **Salão Admin** (painel administrativo para gestão de salões de beleza construído em **Blazor WebAssembly / .NET 9 + MudBlazor**) em uma arquitetura moderna de **microsserviços em contêineres (Docker / Kubernetes)**, **processamento assíncrono com mensageria e função Serverless**, **Infraestrutura como Código (IaC)** e **observabilidade completa**.

## Estrutura do Repositório (Entregáveis 0 a 6)

| Pasta | Entregável | Descrição |
| :--- | :--- | :--- |
| [`0-raci/`](./0-raci/RACI.md) | **0 — Matriz RACI** | Responsabilidades da equipe (R, A, C, I) por etapa do projeto. |
| [`1-arquitetura/`](./1-arquitetura/README.md) | **1 — Arquitetura da Solução** | Diagrama arquitetural (PNG + Mermaid), fluxos, funcionalidades, HA, escalabilidade e observabilidade. |
| [`2-aplicacao/`](./2-aplicacao/) | **2 — Código da Aplicação** | Frontend original `SalaoAdmin` (alteração mínima), API Gateway, 3 microsserviços de referência, guia plug-and-play para backend real e função Serverless/Worker. |
| [`3-infraestrutura-iac/`](./3-infraestrutura-iac/) | **3 — Infraestrutura (IaC)** | Terraform (AWS EKS Multi-AZ, RDS, Redis, SQS+DLQ, Lambda, CloudWatch), Kubernetes (HPA, PDB, Probes) e `docker-compose.yml`. |
| [`4-logs-observabilidade/`](./4-logs-observabilidade/) | **4 — Logs e Eventos** | Configurações Prometheus/Loki/Promtail e logs JSON + métricas reais coletadas em execução. |
| [`5-dashboard-monitoramento/`](./5-dashboard-monitoramento/) | **5 — Dashboard Monitorado** | Provisionamento automático do Grafana, JSON do dashboard e visualização das métricas coletadas. |
| [`6-apresentacao/`](./6-apresentacao/) | **6 — Apresentação (PPTX)** | Deck executivo de 15–20 minutos pronto para apresentação (semanas de 30/11 e 07/12). |

## Como Subir Toda a Stack Localmente (Docker Compose)

```bash
cd 3-infraestrutura-iac
docker compose up -d --build
```

Após subir os contêineres:
- **Aplicação Web (Salão Admin via API Gateway):** `http://localhost:8080`
- **Grafana (Dashboards já provisionados):** `http://localhost:3000` (`admin` / `admin`)
- **Prometheus (Métricas e Alertas):** `http://localhost:9090`
- **RabbitMQ Management (Filas de Mensageria):** `http://localhost:15672` (`salao` / `salao123`)

> **Caso o repositório do Backend oficial seja entregue a tempo:** consulte [`2-aplicacao/backend/COMO_PLUGAR_BACKEND_REAL.md`](./2-aplicacao/backend/COMO_PLUGAR_BACKEND_REAL.md) para apontar o API Gateway diretamente para a API real sem alterar o frontend.
