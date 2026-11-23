# 0. Matriz RACI

Quem faz o quê em cada etapa do projeto.

- **R (Responsável):** executa a etapa.
- **A (Aprovador):** confere e dá o ok final. Cada etapa tem um único aprovador.
- **C (Consultado):** ajuda com dúvidas e revisões.
- **I (Informado):** acompanha o andamento.

## Papéis

| Sigla | Papel | O que cuida |
| :--- | :--- | :--- |
| ARQ | Arquitetura | Escopo, desenho da solução e decisões técnicas. |
| FE | Frontend | Blazor, contêiner do frontend e integração com a API. |
| BE | Backend | Microsserviços, API Gateway e função serverless. |
| OPS | DevOps | Docker Compose, Kubernetes, Terraform e publicação na AWS. |
| MON | Monitoramento | Logs, métricas, alertas e dashboard. |

## Matriz

| Etapa | ARQ | FE | BE | OPS | MON |
| :--- | :-: | :-: | :-: | :-: | :-: |
| 0. Escopo, requisitos e RACI | A/R | C | C | C | C |
| 1.1 Desenho da arquitetura | A/R | C | C | C | I |
| 1.2 README da arquitetura (HA e monitoramento) | A | C | C | R | R |
| 2.1 Contêiner do frontend | I | A/R | I | C | I |
| 2.2 Microsserviços e API Gateway | C | C | A/R | C | I |
| 2.3 Função serverless e fila de eventos | C | I | A/R | C | C |
| 3.1 Docker Compose e Kubernetes | C | I | C | A/R | C |
| 3.2 Terraform (AWS) | C | I | I | A/R | C |
| 3.3 Publicação na EC2 | I | C | C | A/R | C |
| 4. Logs e eventos | I | I | C | C | A/R |
| 5. Prometheus, Grafana e dashboard | I | I | I | C | A/R |
| 6. Apresentação | A/R | R | R | R | R |
