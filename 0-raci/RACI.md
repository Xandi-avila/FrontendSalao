# 0 — Matriz RACI da Equipe

A matriz abaixo define os papéis e responsabilidades da equipe em cada etapa do projeto **Salão Admin em Nuvem**.

**Legenda:**
- **R (Responsible / Responsável):** Executa a atividade.
- **A (Accountable / Aprovador):** Responde pela entrega final e aprovação da etapa (apenas um por etapa).
- **C (Consulted / Consultado):** Fornece insumos técnicos e valida decisões.
- **I (Informed / Informado):** Mantido atualizado sobre o andamento e conclusões.

> **Nota para a equipe:** Substitua os campos `[Nome do Integrante]` abaixo pelos nomes reais dos membros do grupo antes da entrega final.

## Papéis da Equipe

| Sigla | Papel no Projeto | Integrante |
| :--- | :--- | :--- |
| **ARQ** | Arquiteto de Soluções Cloud | `[Nome do Integrante — Arquiteto Cloud]` |
| **DEV-FE** | Desenvolvedor Frontend (Blazor/.NET 9) | `[Nome do Integrante — Frontend]` |
| **DEV-BE** | Desenvolvedor Backend, Microsserviços & Serverless | `[Nome do Integrante — Backend/Serverless]` |
| **DEVOPS** | Engenheiro DevOps / IaC (Docker, K8s, Terraform) | `[Nome do Integrante — DevOps/IaC]` |
| **SRE** | Engenheiro de Observabilidade, QA & Monitoramento | `[Nome do Integrante — SRE/Observabilidade]` |

## Matriz de Responsabilidades por Etapa

| # | Etapa / Entregável | ARQ | DEV-FE | DEV-BE | DEVOPS | SRE |
| :-: | :--- | :-: | :-: | :-: | :-: | :-: |
| **0** | Definição de escopo, requisitos e Matriz RACI | **A/R** | C | C | C | C |
| **1.1** | Desenho da arquitetura (microsserviços, mensageria e serverless) | **A/R** | C | C | C | I |
| **1.2** | Documentação técnica (README, HA, escalabilidade e observabilidade) | **A** | C | C | **R** | **R** |
| **2.1** | Adaptação mínima e containerização do Frontend (`SalaoAdmin`) | I | **A/R** | I | C | I |
| **2.2** | Separação/implementação dos Microsserviços e API Gateway | C | C | **A/R** | C | I |
| **2.3** | Implementação da Função Serverless + Mensageria (SQS/RabbitMQ) | C | I | **A/R** | C | C |
| **3.1** | Infraestrutura como Código — Docker Compose e Manifestos K8s | C | I | C | **A/R** | C |
| **3.2** | Infraestrutura como Código — Terraform (AWS Multi-AZ, EKS, RDS, Lambda) | C | I | I | **A/R** | C |
| **4** | Instrumentação de logs JSON, eventos assíncronos e validação | I | I | C | C | **A/R** |
| **5** | Configuração Prometheus/Loki/Grafana e coleta do print do Dashboard | I | I | I | C | **A/R** |
| **6** | Elaboração da apresentação (PPTX) e ensaio (15–20 min) | **A/R** | **R** | **R** | **R** | **R** |
