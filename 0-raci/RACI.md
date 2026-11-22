# 0 — Matriz RACI da Equipe

Distribuição de papéis e responsabilidades nas etapas do projeto **Salão Admin em Nuvem**.

**Legenda:**
- **R (Responsible):** Executa a entrega.
- **A (Accountable):** Responde pela validação e aprovação da etapa.
- **C (Consulted):** Apoia tecnicamente e revisa decisões.
- **I (Informed):** Acompanha o andamento e resultados.

## Papéis do Projeto

| Sigla | Papel | Responsável |
| :--- | :--- | :--- |
| **ARQ** | Arquiteto de Soluções Cloud | — |
| **DEV-FE** | Desenvolvedor Frontend (Blazor / .NET 9) | — |
| **DEV-BE** | Desenvolvedor Backend, Microsserviços & Serverless | — |
| **DEVOPS** | Engenheiro DevOps / IaC (Docker, Kubernetes, Terraform) | — |
| **SRE** | Observabilidade, QA & Monitoramento | — |

## Responsabilidades por Etapa

| # | Etapa / Entregável | ARQ | DEV-FE | DEV-BE | DEVOPS | SRE |
| :-: | :--- | :-: | :-: | :-: | :-: | :-: |
| **0** | Definição de escopo, requisitos e Matriz RACI | **A/R** | C | C | C | C |
| **1.1** | Desenho da arquitetura (microsserviços, mensageria e serverless) | **A/R** | C | C | C | I |
| **1.2** | Documentação técnica (README, HA, escalabilidade e observabilidade) | **A** | C | C | **R** | **R** |
| **2.1** | Containerização do Frontend (`SalaoAdmin`) | I | **A/R** | I | C | I |
| **2.2** | Implementação dos Microsserviços e API Gateway | C | C | **A/R** | C | I |
| **2.3** | Função Serverless e integração com mensageria (SQS/RabbitMQ) | C | I | **A/R** | C | C |
| **3.1** | Infraestrutura como Código — Docker Compose e Kubernetes | C | I | C | **A/R** | C |
| **3.2** | Infraestrutura como Código — Terraform (AWS Multi-AZ) | C | I | I | **A/R** | C |
| **4** | Instrumentação de logs JSON, eventos assíncronos e validação | I | I | C | C | **A/R** |
| **5** | Configuração Prometheus/Loki/Grafana e dashboard monitorado | I | I | I | C | **A/R** |
| **6** | Elaboração da apresentação técnica | **A/R** | **R** | **R** | **R** | **R** |
