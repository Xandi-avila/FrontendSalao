# Como Plugar o Repositório do Backend Oficial (Plug-and-Play)

Este projeto já inclui 3 microsserviços funcionais (`ms-cadastros`, `ms-agendamentos`, `ms-vendas-analise`) e o worker serverless (`serverless-notificacoes`) compatíveis com as rotas exatas do `SalaoAdmin`.

Caso você receba o repositório oficial do backend antes da apresentação, você pode integrá-lo em **menos de 2 minutos** sem alterar uma única linha do frontend Blazor:

## Opção A — Backend já hospedado (ex: Render `https://salao-beleza-service.onrender.com/`)
No arquivo `2-aplicacao/backend/api-gateway/nginx.conf`, substitua os blocos `location ~ ^/api/...` por:

```nginx
location /api/ {
    proxy_pass https://salao-beleza-service.onrender.com/;
    proxy_ssl_server_name on;
    proxy_set_header Host salao-beleza-service.onrender.com;
    proxy_set_header X-Correlation-ID $request_id;
}
```

## Opção B — Backend oficial rodando em contêiner local no Docker Compose
1. Copie o código do backend oficial para `2-aplicacao/backend/backend-oficial/` com o respectivo `Dockerfile`.
2. Em `3-infraestrutura-iac/docker-compose.yml`, substitua o serviço `ms-cadastros` (ou adicione `backend-oficial`) apontando o `build: ../2-aplicacao/backend/backend-oficial`.
3. Em `2-aplicacao/backend/api-gateway/nginx.conf`, direcione o `upstream` para `backend-oficial:<porta>`.
4. Mantenha o serviço `serverless-notificacoes` e a fila `rabbitmq` ativos para preservar o requisito obrigatório de mensageria/serverless do trabalho!
