# Salão Admin — Arquitetura em Nuvem

Repositório do projeto de **Arquitetura em Nuvem** do sistema **Salão Admin** (painel administrativo para gestão de salões de beleza em **Blazor WebAssembly / .NET 9 + MudBlazor**), estruturado em **microsserviços containerizados (Docker / Kubernetes)**, **mensageria com função Serverless**, **Infraestrutura como Código (IaC)** e **observabilidade completa**.

## Estrutura do Repositório

| Pasta | Etapa | Descrição |
| :--- | :--- | :--- |
| [`0-raci/`](./0-raci/RACI.md) | **0 — Matriz RACI** | Divisão de responsabilidades da equipe por etapa do projeto. |
| [`1-arquitetura/`](./1-arquitetura/README.md) | **1 — Arquitetura da Solução** | Diagrama arquitetural (PNG e Mermaid), fluxos, alta disponibilidade e observabilidade. |
| [`2-aplicacao/`](./2-aplicacao/) | **2 — Código da Aplicação** | Frontend `SalaoAdmin`, API Gateway Nginx, microsserviços de domínio e worker Serverless. |
| [`3-infraestrutura-iac/`](./3-infraestrutura-iac/) | **3 — Infraestrutura (IaC)** | Terraform (AWS EKS Multi-AZ, RDS, Redis, SQS+DLQ, Lambda), manifestos Kubernetes e Docker Compose. |
| [`4-logs-observabilidade/`](./4-logs-observabilidade/) | **4 — Logs e Eventos** | Configurações Prometheus/Loki e registros reais de logs JSON, eventos e métricas. |
| [`5-dashboard-monitoramento/`](./5-dashboard-monitoramento/) | **5 — Dashboard Monitorado** | Provisionamento do Grafana, JSON do dashboard e painel de métricas coletadas. |
| [`6-apresentacao/`](./6-apresentacao/) | **6 — Apresentação** | Apresentação técnica em PowerPoint (`.pptx`) com visão completa da solução. |

## Execução com Docker Compose

```bash
cd 3-infraestrutura-iac
docker compose up -d --build
```

Serviços expostos após a inicialização:
- **Aplicação Web (API Gateway):** `http://localhost:8080`
- **Grafana:** `http://localhost:3000` (`admin` / `admin`)
- **Prometheus:** `http://localhost:9090`
- **RabbitMQ Management:** `http://localhost:15672` (`salao` / `salao123`)

Para apontar o API Gateway para um serviço externo ou backend alternativo, veja [`2-aplicacao/backend/INTEGRACAO_API.md`](./2-aplicacao/backend/INTEGRACAO_API.md).
