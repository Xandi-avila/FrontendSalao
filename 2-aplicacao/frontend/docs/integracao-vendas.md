# Integração — Módulo de Vendas (PDV)

Documentação da API: [https://salao-beleza-service.onrender.com/docs/](https://salao-beleza-service.onrender.com/docs/)

## Endpoints utilizados

| Método | Rota | Uso no front |
|--------|------|--------------|
| GET | `/vendas` | Histórico e indicadores (pagina, tamanho, funcionarioId, clienteId) |
| POST | `/vendas` | Registrar venda no checkout |
| GET | `/vendas/{id}` | Detalhe da venda |
| GET | `/produtos` | Catálogo PDV |
| GET | `/servicos` | Catálogo PDV |
| GET | `/clientes` | Autocomplete de cliente |

## Contrato `VendaCadastro` (POST)

```json
{
  "funcionarioId": "uuid",
  "clienteId": "uuid | null",
  "dataHora": "ISO8601 UTC (opcional)",
  "itens": [
    {
      "tipo": "PRODUTO | SERVICO",
      "produtoId": "uuid | null",
      "servicoId": "uuid | null",
      "quantidade": 1,
      "valorUnitario": 0
    }
  ]
}
```

O **total é calculado no servidor** — o front exibe estimativa local.

## O que a API **não** expõe (fallback no front)

| Funcionalidade | Situação |
|----------------|----------|
| Status da venda | Exibido como **Registrada** (campo não existe em `Venda`) |
| Forma de pagamento | Tela fictícia — **não é enviada** à API |
| Desconto / cupom / comissão | Estrutura no carrinho preparada, **não enviada** |
| Filtro por período em GET /vendas | **Filtro local** após buscar páginas |
| Filtro por tipo produto/serviço | **Filtro local** |
| PUT/DELETE venda | **Não existe** — vendas são somente leitura após registro |
| Agendamento → Venda automático | `VendaAgendamentoBridge` preparado, **sem endpoint** |
| WhatsApp / e-mail pós-venda | UI preparada, **não implementado** |
| Dashboard financeiro avançado | Placeholder visual apenas |

## Filtros GET /vendas

**Na API:** `pagina`, `tamanho`, `funcionarioId`, `clienteId`

**Somente no front:** `inicio`, `fim`, `tipoItem`, busca por número

## Testes com mock

Em `appsettings.Development.json`: `"UsarMocks": true` — vendas ficam em memória (`VendaServicoMock`).

## Testes com API real

`"UsarMocks": false` — requer JWT válido e CORS configurado (ver `docs/integracao-api-cors.md`).
