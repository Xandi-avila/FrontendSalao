# Integração do API Gateway com Serviços Externos ou Alternativos

O API Gateway (`2-aplicacao/backend/api-gateway/nginx.conf`) centraliza o roteamento das chamadas `/api/*` do frontend `SalaoAdmin`, permitindo alternar os destinos de upstream sem modificar o código Blazor.

## Roteamento para API Externa (ex.: Render)

Para redirecionar todas as rotas `/api/*` para uma instância externa já publicada, ajuste o bloco de roteamento em `2-aplicacao/backend/api-gateway/nginx.conf`:

```nginx
location /api/ {
    proxy_pass https://salao-beleza-service.onrender.com/;
    proxy_ssl_server_name on;
    proxy_set_header Host salao-beleza-service.onrender.com;
    proxy_set_header X-Correlation-ID $request_id;
}
```

## Roteamento para Serviço em Contêiner Dedicado

1. Adicione o código-fonte e o `Dockerfile` do serviço em uma subpasta dentro de `2-aplicacao/backend/`.
2. Registre o serviço em `3-infraestrutura-iac/docker-compose.yml` na rede `salao-net`.
3. Atualize o `upstream` correspondente no `nginx.conf` e recrie os contêineres com `docker compose up -d --build`.
