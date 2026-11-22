# 1 — Arquitetura da Solução: Salão Admin em Nuvem

![Diagrama de Arquitetura Salão Admin](./diagrama-arquitetura-salao.png)

> O código-fonte editável do diagrama também está disponível em [`diagrama-arquitetura.mmd`](./diagrama-arquitetura.mmd) (compatível com renderização nativa do GitHub).

---

## 1. Visão Geral da Aplicação (`SalaoAdmin`)

O **Salão Admin** é uma aplicação administrativa para gestão completa de salões de beleza, construída em **Blazor WebAssembly (.NET 9)** com **MudBlazor 9**, **FluentValidation 12** e autenticação via **JWT**.

A partir da análise do repositório original (`FrontendSalao`), o frontend já opera desacoplado via `UrlBaseApi: "/api/"` (`SalaoAdmin/wwwroot/appsettings.json`) e consome os seguintes domínios funcionais:

| Domínio / Módulo no Frontend | Rotas HTTP Consumidas pelo `SalaoAdmin` | Microsserviço Responsável |
| :--- | :--- | :--- |
| **Autenticação & Cadastros** (`Login`, `Clientes`, `Funcionários`, `Categorias`, `Produtos`, `Serviços`) | `POST /api/auth/login`, CRUD `/api/clientes`, `/api/funcionarios`, `/api/categorias`, `/api/produtos`, `/api/servicos` | `ms-cadastros` |
| **Agenda & Agendamentos** (`Agenda`, `Agendamentos`) | `GET/PUT /api/agenda/funcionarios/{id}`, `DELETE /api/agenda/janelas/{id}`, CRUD `/api/agendamentos`, `PATCH /api/agendamentos/{id}/cancelar` | `ms-agendamentos` |
| **PDV, Vendas & Análise** (`Home`, `Vendas`, `Histórico`, `Detalhe`) | `GET/POST /api/vendas`, `GET /api/vendas/{id}`, `GET /api/analise/faturamento`, `/api/analise/funcionarios`, `/api/analise/servicos` | `ms-vendas-analise` |
| **Notificações & Auditoria Assíncrona** | Consumo de eventos `agendamento.criado`, `agendamento.cancelado`, `venda.concluida` | `notificacao-worker-lambda` (Serverless + Fila) |

---

## 2. Ambiente e Topologia em Nuvem

A solução foi desenhada para rodar de forma consistente em **dois ambientes** com a mesma topologia lógica:

1. **Produção / Nuvem (AWS — provisionado via Terraform em `3-infraestrutura-iac/terraform/`):**
   - **Borda e Entrega Estática:** Amazon CloudFront + AWS WAF + Application Load Balancer (ALB) com TLS 1.3 (ACM).
   - **Orquestração de Microsserviços:** **Amazon EKS (Kubernetes gerenciado)** distribuído em **3 Zonas de Disponibilidade (Multi-AZ)** dentro de sub-redes privadas.
   - **Mensageria e Serverless:** **Amazon SQS** (fila principal `salao-eventos-queue` + Dead-Letter Queue `salao-eventos-dlq`) integrados via *Event Source Mapping* à função **AWS Lambda (`notificacao-worker-lambda`)**.
   - **Camada de Dados:** **Amazon RDS PostgreSQL (Multi-AZ)** com bancos/schemas isolados por microsserviço (*Database-per-Service*) e **Amazon ElastiCache (Redis)** para cache de catálogo, dashboard analítico e proteção contra picos de leitura.
2. **Desenvolvimento / Demonstração Local (Docker Compose em `3-infraestrutura-iac/docker-compose.yml`):**
   - **Nginx API Gateway** na porta `8080` servindo o SPA Blazor WASM (`/`) e roteando `/api/*` para os 3 microsserviços.
   - **RabbitMQ** (substituto local do SQS) + contêiner do worker `serverless-notificacoes`, **PostgreSQL**, **Redis** e stack completa **Prometheus + Loki + Promtail + Grafana**.

---

## 3. Fluxos da Aplicação

### 3.1 Fluxo Síncrono (Leitura e Operações Transacionais)
1. O usuário acessa o painel **Salão Admin** no navegador (SPA Blazor WebAssembly baixado do Nginx/CloudFront).
2. Ao realizar login (`POST /api/auth/login`), o `ms-cadastros` valida as credenciais e retorna um token **JWT** assinado.
3. As requisições subsequentes carregam `Authorization: Bearer <token>` (injetado pelo `AuthHttpMessageHandler` do Blazor) e um `X-Correlation-ID` propagado pelo API Gateway.
4. O API Gateway / Ingress roteia o tráfego pelo prefixo da rota (`/api/clientes`, `/api/agendamentos`, `/api/vendas`, `/api/analise`) para o microsserviço correspondente.
5. Consultas frequentes de leitura (como indicadores de faturamento e catálogos) verificam primeiro o **Redis** (padrão *Cache-Aside*) antes de consultar o **PostgreSQL**.

### 3.2 Fluxo Assíncrono (Mensageria + Função Serverless)
1. Quando um agendamento é criado (`POST /api/agendamentos`), cancelado (`PATCH /api/agendamentos/{id}/cancelar`) ou uma venda é finalizada (`POST /api/vendas`), o microsserviço persiste a transação no banco e **publica um evento JSON** na fila **Amazon SQS** (ou **RabbitMQ** no ambiente local) sem bloquear a resposta HTTP ao usuário.
2. A função **AWS Lambda (`notificacao-worker-lambda`)** é disparada automaticamente pela fila, processa o evento de forma idempotente (enviando confirmação/lembrete ao cliente e registrando auditoria) e emite logs estruturados e métricas de processamento.
3. Em caso de falha após 3 tentativas (*MaxReceiveCount = 3*), a mensagem é redirecionada automaticamente para a **Dead-Letter Queue (DLQ)**, disparando alerta no CloudWatch/Grafana para reprocessamento seguro sem perda de dados.

---

## 4. Estratégia de Alta Disponibilidade (HA) e Escalabilidade

| Camada | Mecanismo de Alta Disponibilidade (HA) | Mecanismo de Escalabilidade |
| :--- | :--- | :--- |
| **Rede & Entrada** | ALB Multi-AZ com healthchecks ativos (`/health`), proteção contra falhas de zona. | Balanceamento automático horizontal com elástico nativo AWS ALB / CloudFront. |
| **Frontend (Blazor WASM)** | Mínimo de 2 réplicas estáticas distribuídas entre AZs + `PodDisruptionBudget` (`minAvailable: 1`). | Escala via CDN (CloudFront) e HPA por CPU/memória nos pods Nginx. |
| **Microsserviços (EKS)** | Mínimo de **2 réplicas por microsserviço** em AZs distintas (`topologySpreadConstraints`), `readinessProbe` e `livenessProbe` em `/ready` e `/health`, e `PodDisruptionBudget`. | **Horizontal Pod Autoscaler (HPA)** escalando de 2 até 10 réplicas quando CPU > 70% ou Memória > 80%; **Cluster Autoscaler / Karpenter** para novos nós EC2. |
| **Mensageria & Serverless** | Amazon SQS replicado nativamente entre múltiplas AZs + **DLQ** para tolerância a falhas e desacoplamento temporal. | **AWS Lambda** escala automaticamente por concorrência conforme a profundidade da fila SQS (zero gerenciamento de servidor). |
| **Banco de Dados & Cache** | **Amazon RDS PostgreSQL Multi-AZ** (replicação síncrona para standby com failover automático em ~60s) + backups automatizados (retenção de 7 dias). **Redis** em modo Replication Group Multi-AZ. | *Read Replicas* para consultas pesadas de `ms-vendas-analise` e cache distribuído no Redis reduzindo carga no banco primário. |

---

## 5. Monitoramento e Observabilidade (Os 3 Pilares)

A arquitetura implementa observabilidade nativa baseada nos **três pilares** (Métricas, Logs e Traces) e nos **Four Golden Signals** (Latência, Tráfego, Erros e Saturação):

1. **Métricas (Prometheus + Grafana + AWS CloudWatch):**
   - Cada microsserviço e o worker serverless expõem o endpoint `/metrics` no formato padrão OpenMetrics/Prometheus.
   - Métricas coletadas: `http_requests_total` (por serviço, rota, método e status), `http_request_duration_ms` (latência média e percentil p95), `serverless_events_processed_total` (por tipo de evento e status) e métricas de infraestrutura (CPU, RAM, profundidade da fila SQS/RabbitMQ).
   - **Alertas ativos (`4-logs-observabilidade/prometheus/alerts.yml`):** taxa de erros HTTP 5xx > 5% por 2 minutos, latência p95 > 500ms, instância indisponível (`up == 0`) e acúmulo de mensagens na DLQ.
2. **Logs Estruturados Centralizados (JSON + Promtail + Grafana Loki / CloudWatch Logs):**
   - Todos os serviços escrevem logs estruturados em formato **JSON** em `stdout` contendo `timestamp`, `level`, `service`, `correlation_id`, `method`, `path`, `status_code`, `latency_ms` e `event_type`.
   - O **Promtail** coleta os logs dos contêineres e os envia ao **Grafana Loki** (e **CloudWatch Logs** na AWS), permitindo correlação instantânea com os gráficos de métricas no Grafana.
3. **Rastreamento Distribuído (Correlation ID / OpenTelemetry / AWS X-Ray):**
   - O API Gateway injeta o cabeçalho `X-Correlation-ID` em toda requisição de entrada. Esse identificador percorre os microsserviços, é anexado ao payload do evento na fila SQS/RabbitMQ e registrado pela função Serverless, garantindo rastreabilidade ponta a ponta de qualquer transação.
