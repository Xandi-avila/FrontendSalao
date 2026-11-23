# 4 — Logs, eventos e métricas

Aqui ficam a configuração da coleta e os registros de uma execução de teste do sistema completo (três microsserviços e a função de notificações).

## Configuração

- `prometheus/prometheus.yml`: lê o `/metrics` de cada serviço a cada 15 segundos.
- `prometheus/alerts.yml`: dois alertas, um para serviço fora do ar por mais de 1 minuto e outro para mais de 5% de respostas 5xx.
- `loki/loki-config.yaml`: configuração do Loki, que guarda os logs para o Grafana.

## Registros da execução

- `logs/aplicacao-execucao-real.jsonl`: um log em JSON por requisição, com data e hora, nível, serviço, `correlation_id`, método, rota, status e tempo de resposta. Cobre login, cadastros, agendamentos, vendas, relatórios e alguns erros esperados (401 e 404).
- `logs/eventos-mensageria-serverless.jsonl`: eventos `agendamento.criado`, `agendamento.cancelado` e `venda.concluida` enviados pelos microsserviços e tratados pela função de notificações. Como o `correlation_id` se repete, dá para seguir uma ação do usuário do começo ao fim.
- `metricas/prometheus-metrics-snapshot.txt`: o que os endpoints `/metrics` devolviam no momento do teste.

## Como ver os logs no servidor

```bash
cd 3-infraestrutura-iac
docker compose logs -f ms-agendamentos
```

O mesmo vale para `ms-cadastros`, `ms-vendas-analise`, `serverless-notificacoes` e `api-gateway`.
