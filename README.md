# Salão Admin na Nuvem

Trabalho da disciplina de **Arquitetura em Nuvem**. O sistema é o **Salão Admin**, um painel administrativo para salão de beleza feito em **Blazor WebAssembly (.NET 9) + MudBlazor**. Nós o colocamos para rodar em **microsserviços em contêineres**, com uma **fila de eventos e uma função serverless**, **infraestrutura como código** e **monitoramento com Prometheus e Grafana**.

## Estrutura do repositório

| Pasta | Entregável | Conteúdo |
| :--- | :--- | :--- |
| [`0-raci/`](./0-raci/RACI.md) | 0. Matriz RACI | Papéis da equipe e responsabilidades por etapa. |
| [`1-arquitetura/`](./1-arquitetura/README.md) | 1. Arquitetura | Diagrama, fluxos, alta disponibilidade e monitoramento. |
| [`2-aplicacao/`](./2-aplicacao/) | 2. Código da aplicação | Frontend, API Gateway, 3 microsserviços e a função serverless. |
| [`3-infraestrutura-iac/`](./3-infraestrutura-iac/) | 3. Infraestrutura como código | Docker Compose, manifestos Kubernetes e Terraform (AWS). |
| [`4-logs-observabilidade/`](./4-logs-observabilidade/README.md) | 4. Logs e eventos | Configuração do Prometheus e do Loki, logs JSON e métricas coletadas. |
| [`5-dashboard-monitoramento/`](./5-dashboard-monitoramento/README.md) | 5. Dashboard | Dashboard do Grafana e print do painel rodando na AWS. |
| [`6-apresentacao/`](./6-apresentacao/) | 6. Apresentação | Slides da apresentação (`.pptx`), com roteiro nas notas. |

## Como rodar

Com Docker instalado:

```bash
cd 3-infraestrutura-iac
docker compose up -d --build
```

O comando sobe 12 contêineres. Depois de alguns minutos (o build do .NET demora na primeira vez):

- **Sistema:** `http://localhost:8080`
- **Grafana:** `http://localhost:3000` (usuário `admin`, senha `admin`)
- **Prometheus:** `http://localhost:9090`
- **RabbitMQ:** `http://localhost:15672` (usuário `salao`, senha `salao123`)

Para conferir se tudo subiu: `docker compose ps` (todos devem aparecer como `healthy` ou `Up`).

## Ambiente na AWS usado na demonstração

O sistema foi publicado numa **EC2 do AWS Academy** (Ubuntu, `t3.micro`, região `us-east-1`) com o mesmo `docker compose up -d --build`.

- No **Security Group** liberamos as portas 8080 (sistema), 3000 (Grafana), 9090 (Prometheus) e 15672 (RabbitMQ).
- A `t3.micro` tem 1 GB de memória. Para o build do .NET 9 funcionar, criamos **2 GB de swap**:

  ```bash
  sudo fallocate -l 2G /swapfile && sudo chmod 600 /swapfile
  sudo mkswap /swapfile && sudo swapon /swapfile
  echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab   # mantém o swap depois de reiniciar
  ```

- O IP público da EC2 muda a cada vez que o laboratório é reiniciado.

O laboratório não permite criar EKS, RDS e Lambda com perfis próprios. Por isso a demonstração usa Docker Compose numa EC2, e os arquivos de Terraform e Kubernetes descrevem o ambiente de produção.

## Backend

O repositório original tinha só o frontend. Para o sistema funcionar, criamos três microsserviços simples em Python que respondem às mesmas rotas e no mesmo formato que o frontend espera. Para usar outra API no lugar deles, veja [`2-aplicacao/backend/INTEGRACAO_API.md`](./2-aplicacao/backend/INTEGRACAO_API.md).
