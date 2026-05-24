# Integração API — CORS e primeiro usuário

## Problema confirmado (teste em 24/05/2026)

| Teste | Resultado |
|-------|-----------|
| `GET /saude` | 200 OK |
| `POST /auth/login` (credenciais do usuário) | 401 — credenciais inválidas (usuário ainda não existe) |
| `POST /funcionarios` (sem token) | 401 — token não informado |
| Header `Access-Control-Allow-Origin` na resposta | **ausente** |
| `OPTIONS /funcionarios` | 401 — exige token (incorreto para preflight) |

O front em `http://localhost:5086` e o Swagger no navegador falham com **Failed to fetch** porque o **back-end não libera CORS** para o browser.

Isso **não se corrige no front-end** de forma definitiva. Quem mantém a API precisa habilitar CORS.

---

## O que o back-end precisa fazer (Node/Express exemplo)

```javascript
const cors = require('cors');

app.use(cors({
  origin: [
    'http://localhost:5086',
    'https://seu-front-publicado.com'
  ],
  methods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'Authorization'],
  credentials: true
}));

// OPTIONS não pode exigir JWT
app.options('*', cors());
```

Regras importantes:

1. Responder `OPTIONS` **sem** autenticação em todas as rotas.
2. Incluir `Access-Control-Allow-Origin` nas respostas reais (GET/POST/etc.).
3. Adicionar no Swagger/OpenAPI o server: `https://salao-beleza-service.onrender.com` (hoje só aparece `http://localhost:3000`, o que quebra o "Try it out" no Render).

---

## Como criar o primeiro usuário

`POST /funcionarios` exige **JWT** e **nível >= 3** (Gerente/Administrador).

Sem um usuário inicial no banco, é impossível criar o primeiro admin só pelo front ou pelo Swagger no browser.

Opções (back-end / infra):

1. **Seed** na migration ou script de deploy (recomendado).
2. Inserção manual no banco (senha com hash bcrypt/argon).
3. Endpoint temporário `POST /auth/register` só em ambiente de homologação.

### Body correto para cadastro (quando já tiver token)

```json
{
  "nomeCompleto": "alexandre",
  "endereco": "teste",
  "telefone": "997721371",
  "profissaoCargo": "adm",
  "email": "xandiipereiraa@outlook.com",
  "senha": "236412001",
  "dataNascimento": "1998-02-08T00:00:00Z",
  "nivelPermissao": 3,
  "status": 1
}
```

`dataNascimento` deve ser **ISO 8601** (`1998-02-08` ou com hora), não `08021998`.

### Testar API sem CORS (PowerShell — só servidor)

```powershell
# 1) Depois que existir usuário no banco:
$login = Invoke-RestMethod -Uri "https://salao-beleza-service.onrender.com/auth/login" `
  -Method POST -ContentType "application/json" `
  -Body '{"email":"SEU_EMAIL","senha":"SUA_SENHA"}'

$token = $login.dados.token

# 2) Criar funcionário (com token de nível >= 3):
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "https://salao-beleza-service.onrender.com/funcionarios" `
  -Method POST -ContentType "application/json" -Headers $headers `
  -Body '{"nomeCompleto":"...","email":"...","senha":"...","profissaoCargo":"adm","nivelPermissao":3,"status":1}'
```

---

## Swagger "Failed to fetch"

No topo do Swagger, altere o servidor de `http://localhost:3000` para:

`https://salao-beleza-service.onrender.com`

Se continuar com CORS no browser, use Postman/Insomnia ou PowerShell (acima).

---

## Trabalhar no front enquanto a API não está pronta

Com `dotnet run` (perfil Development), o arquivo `wwwroot/appsettings.Development.json` usa **`UsarMocks: true`**.

Para voltar a testar a API real, apague esse arquivo ou defina `"UsarMocks": false` depois que CORS e o usuário inicial estiverem ok.
