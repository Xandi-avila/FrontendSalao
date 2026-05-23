# Salão Admin

Front-end do painel administrativo de um salão de beleza.

Blazor WebAssembly (.NET 9), MudBlazor e FluentValidation. Os cadastros usam dados mockados por enquanto; a ideia é plugar a API depois.

## Como rodar

```bash
cd SalaoAdmin
dotnet run
```

No perfil padrão (`http`) sobe em **http://localhost:5086**.

## Pastas principais

| Pasta | O que tem |
|-------|-----------|
| `Pages/` | Telas de listagem, cadastro e edição |
| `Components/` | Formulários, tabelas e componentes reutilizáveis |
| `Services/Mock/` | Serviços com dados em memória |
| `Services/Api/` | Cliente HTTP (integração com API) |
| `Interfaces/` | Contratos dos serviços |
| `DTOs/` | Modelos de leitura e formulário |
| `Validators/` | Validações dos formulários |
| `MockData/` | Dados iniciais para desenvolvimento |
| `Layout/` | Menu lateral e tema |

## Rotas

- `/` — início
- `/funcionarios`
- `/clientes`
- `/produtos`
- `/categorias`
- `/servicos`

## Configuração

Arquivo: `SalaoAdmin/wwwroot/appsettings.json`

```json
{
  "ConfiguracaoApi": {
    "UrlBaseApi": "https://sua-api.com/api/",
    "UsarMocks": true,
    "TimeoutSegundos": 30
  }
}
```

Com `UsarMocks: true` usa os mocks. Com `false`, o projeto tenta a API (hoje só funcionário tem esqueleto em `Services/Api/`).

## Publicar

```bash
cd SalaoAdmin
dotnet publish -c Release -o ./publish
```

Hospeda o conteúdo de `publish/wwwroot` como site estático. Precisa de fallback para `index.html` (SPA).

Exemplo Nginx:

```nginx
location / {
    root /var/www/salao-admin;
    try_files $uri $uri/ /index.html;
}
```

No IIS, aponta o site para `publish/wwwroot` e mantém o rewrite para `index.html`.

## Ligar na API

1. `UsarMocks: false` no appsettings
2. Implementar os serviços em `Services/Api/`
3. Registrar em `Shared/RegistroServicos.cs` no lugar dos mocks

Respostas esperadas: `Resultado` / `Resultado<T>`, listagens com `FiltroPaginacao` e `ListaPaginada<T>`.

## Stack

- .NET 9 / Blazor WASM
- MudBlazor 9
- FluentValidation 12
