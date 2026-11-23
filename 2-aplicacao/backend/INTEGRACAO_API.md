# Trocar o backend por outra API

O frontend chama sempre `/api/...`. Quem decide para onde cada chamada vai é o API Gateway (`2-aplicacao/backend/api-gateway/nginx.conf`). Por isso dá para trocar os microsserviços de demonstração por outra API sem mexer no código do Blazor.

A outra API precisa responder no mesmo formato que o frontend espera:

```json
{ "sucesso": true, "mensagem": "OK", "erros": [], "dados": { } }
```

Nas listas, `dados` vem como `{ "pagina", "tamanho", "total", "itens" }`, e os IDs são GUID.

## Usar uma API publicada na internet (ex.: Render)

No `nginx.conf`, **apague os três blocos** `location ~ ^/api/...` (cadastros, agendamentos e vendas) e coloque no lugar:

```nginx
location /api/ {
    proxy_pass https://salao-beleza-service.onrender.com/;
    proxy_ssl_server_name on;
    proxy_set_header Host salao-beleza-service.onrender.com;
    proxy_set_header X-Correlation-ID $request_id;
}
```

Os blocos antigos precisam sair porque, no Nginx, um `location` com expressão regular tem prioridade sobre um `location /api/` comum.

Depois reinicie o gateway: `docker compose restart api-gateway`.

## Usar uma API em outro contêiner

1. Coloque o código e o `Dockerfile` da API numa pasta dentro de `2-aplicacao/backend/`.
2. Acrescente o serviço no `3-infraestrutura-iac/docker-compose.yml`, na rede `salao-net`.
3. Aponte o `upstream` correspondente no `nginx.conf` para o novo serviço e rode `docker compose up -d --build`.
