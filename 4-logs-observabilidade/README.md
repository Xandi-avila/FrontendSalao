# 4 — Registros de Logs, Eventos e Métricas (Validação de Observabilidade)

Esta pasta reúne as configurações de coleta (**Prometheus** e **Loki**) e os **registros reais gerados durante a execução integrada** dos 3 microsserviços (`ms-cadastros`, `ms-agendamentos`, `ms-vendas-analise`) e da função serverless (`serverless-notificacoes`):

- [`logs/aplicacao-execucao-real.jsonl`](./logs/aplicacao-execucao-real.jsonl): Logs estruturados em JSON com `timestamp`, `level`, `service`, `correlation_id`, `method`, `path`, `status_code` e `latency_ms` cobrindo autenticação JWT, CRUDs, agendamentos, vendas, análises e validação de erros HTTP (`401`/`404`).
- [`logs/eventos-mensageria-serverless.jsonl`](./logs/eventos-mensageria-serverless.jsonl): Eventos assíncronos reais (`agendamento.criado`, `agendamento.cancelado`, `venda.concluida`) publicados pelos microsserviços e consumidos pela função `serverless-notificacoes` com o mesmo `correlation_id` (rastreamento ponta a ponta).
- [`metricas/prometheus-metrics-snapshot.txt`](./metricas/prometheus-metrics-snapshot.txt): Snapshot coletado diretamente dos endpoints `/metrics` de cada serviço em execução.
