# Salão Admin

Front-end do painel administrativo de um salão de beleza.

Blazor WebAssembly (.NET 9), MudBlazor e FluentValidation, integrado à API REST.

## Como rodar

```bash
cd SalaoAdmin
dotnet run
```

No perfil padrão (`http`) sobe em **http://localhost:5086**.

Em Development, o `appsettings.Development.json` aponta direto para a API no Render (CORS precisa liberar `localhost`).

## Pastas principais

| Pasta | O que tem |
|-------|-----------|
| `Pages/` | Telas de listagem, cadastro e edição |
| `Components/` | Formulários, tabelas e componentes reutilizáveis |
| `Services/Api/` | Cliente HTTP (integração com API) |
| `Services/Mock/` | Serviços com dados em memória (fallback opcional) |
| `Interfaces/` | Contratos dos serviços |
| `DTOs/` | Modelos de leitura e formulário |
| `Validators/` | Validações dos formulários |
| `Layout/` | Menu lateral e tema |

## Rotas principais

- `/` — início
- `/login` — autenticação
- `/funcionarios`, `/clientes`, `/produtos`, `/categorias`, `/servicos`
- `/agenda`, `/vendas`, `/vendas/historico`

## Configuração

### Produção (`wwwroot/appsettings.json`)

```json
{
  "ConfiguracaoApi": {
    "UrlBaseApi": "/api/",
    "TimeoutSegundos": 30,
    "UsarMocks": false
  }
}
```

O front chama a **mesma origem** (`/api/...`). O servidor web faz proxy para a API — sem CORS no browser.

### Desenvolvimento local (`wwwroot/appsettings.Development.json`)

```json
{
  "ConfiguracaoApi": {
    "UrlBaseApi": "https://salao-beleza-service.onrender.com/",
    "TimeoutSegundos": 30,
    "UsarMocks": false
  }
}
```

Esse arquivo **não vai no publish** — só vale com `dotnet run`.

## Publicar

```bash
cd SalaoAdmin
dotnet publish -c Release -o ./publish
```

Hospeda o conteúdo de `publish/wwwroot` como site estático. Precisa de fallback SPA e proxy `/api/`.

### Nginx (servidor Linux)

```nginx
location / {
    root /var/www/salao-admin;
    try_files $uri $uri/ /index.html;
}

location /api/ {
    proxy_pass https://salao-beleza-service.onrender.com/;
    proxy_http_version 1.1;
    proxy_set_header Host salao-beleza-service.onrender.com;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
}
```

## Documentação de integração

- `docs/integracao-api-cors.md` — CORS e primeiro usuário
- `docs/integracao-vendas.md` — módulo PDV

## Stack

- .NET 9 / Blazor WASM
- MudBlazor 9
- FluentValidation 12
