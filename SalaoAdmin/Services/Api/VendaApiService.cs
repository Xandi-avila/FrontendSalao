using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Vendas;
using SalaoAdmin.Utilitarios;

namespace SalaoAdmin.Servicos.Api;

public class VendaApiService(
    HttpClient httpClient,
    ErrorHandlerService errorHandler,
    ILogger<VendaApiService> logger) : BaseApiService(httpClient, errorHandler, logger), IVendaServico
{
    public async Task<Resultado<ListaPaginada<VendaDto>>> ListarAsync(FiltroVenda filtro, CancellationToken cancelamento = default)
    {
        var tamanho = Math.Clamp(filtro.ItensPorPagina, 1, 100);
        var temFiltroLocal = filtro.Inicio.HasValue || filtro.Fim.HasValue ||
                             !string.IsNullOrWhiteSpace(filtro.TipoItem) || !string.IsNullOrWhiteSpace(filtro.Busca);

        if (temFiltroLocal)
        {
            var todos = await BuscarTodasPaginasApiAsync(filtro, cancelamento);
            var filtrados = AplicarFiltrosLocais(todos, filtro);
            var pagina = Math.Max(1, filtro.Pagina);
            var itens = filtrados.Skip((pagina - 1) * tamanho).Take(tamanho).ToList();
            return Resultado<ListaPaginada<VendaDto>>.Ok(new ListaPaginada<VendaDto>
            {
                Itens = itens,
                Total = filtrados.Count,
                Pagina = pagina,
                ItensPorPagina = tamanho
            });
        }

        var query = MontarQuery(new Dictionary<string, string?>
        {
            ["pagina"] = filtro.Pagina.ToString(),
            ["tamanho"] = tamanho.ToString(),
            ["funcionarioId"] = filtro.FuncionarioId?.ToString(),
            ["clienteId"] = filtro.ClienteId?.ToString()
        });

        var api = await GetAsync<PaginacaoApi<VendaDto>>($"vendas{query}", cancelamento);
        if (!api.Sucesso || api.Dados is null)
            return Resultado<ListaPaginada<VendaDto>>.Falha(api.Erros.Count > 0 ? api.Erros : [api.Mensagem ?? "Falha ao listar vendas."]);

        return Resultado<ListaPaginada<VendaDto>>.Ok(new ListaPaginada<VendaDto>
        {
            Itens = api.Dados.Itens.OrderByDescending(v => v.DataHora).ToList(),
            Total = api.Dados.Total,
            Pagina = api.Dados.Pagina,
            ItensPorPagina = api.Dados.Tamanho
        });
    }

    private async Task<List<VendaDto>> BuscarTodasPaginasApiAsync(FiltroVenda filtro, CancellationToken cancelamento)
    {
        var todos = new List<VendaDto>();
        var pagina = 1;
        while (true)
        {
            var query = MontarQuery(new Dictionary<string, string?>
            {
                ["pagina"] = pagina.ToString(),
                ["tamanho"] = "100",
                ["funcionarioId"] = filtro.FuncionarioId?.ToString(),
                ["clienteId"] = filtro.ClienteId?.ToString()
            });
            var api = await GetAsync<PaginacaoApi<VendaDto>>($"vendas{query}", cancelamento);
            if (!api.Sucesso || api.Dados is null) break;
            todos.AddRange(api.Dados.Itens);
            if (todos.Count >= api.Dados.Total || api.Dados.Itens.Count < 100) break;
            pagina++;
        }
        return todos;
    }

    public async Task<Resultado<VendaDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await GetAsync<VendaDto>($"vendas/{id}", cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<VendaDto>> RegistrarAsync(VendaCadastroDto dto, CancellationToken cancelamento = default)
    {
        var payload = NormalizarCadastro(dto);
        var api = await PostAsync<VendaCadastroDto, VendaDto>("vendas", payload, cancelamento);
        return api.ParaResultado();
    }

    private static VendaCadastroDto NormalizarCadastro(VendaCadastroDto dto)
    {
        DateTime? dataHora = dto.DataHora;
        if (dataHora.HasValue)
        {
            var dt = dataHora.Value;
            if (dt.Kind == DateTimeKind.Unspecified)
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            dataHora = dt.ToUniversalTime();
        }

        return new VendaCadastroDto
        {
            FuncionarioId = dto.FuncionarioId,
            ClienteId = dto.ClienteId,
            DataHora = dataHora,
            Itens = dto.Itens.Select(i => new ItemVendaCadastroDto
            {
                Tipo = i.Tipo,
                ProdutoId = i.ProdutoId,
                ServicoId = i.ServicoId,
                Quantidade = Math.Max(1, i.Quantidade),
                ValorUnitario = i.ValorUnitario
            }).ToList()
        };
    }

    private static List<VendaDto> AplicarFiltrosLocais(IEnumerable<VendaDto> itens, FiltroVenda filtro)
    {
        var query = itens.AsEnumerable();

        if (filtro.Inicio.HasValue || filtro.Fim.HasValue)
            query = query.Where(v => VendaCalculoHelper.VendaNoPeriodo(v, filtro.Inicio, filtro.Fim));

        if (!string.IsNullOrWhiteSpace(filtro.TipoItem))
            query = query.Where(v => VendaCalculoHelper.VendaContemTipo(v, filtro.TipoItem));

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim().ToLowerInvariant();
            query = query.Where(v => v.Id.ToString().Contains(busca, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderByDescending(v => v.DataHora).ToList();
    }
}
