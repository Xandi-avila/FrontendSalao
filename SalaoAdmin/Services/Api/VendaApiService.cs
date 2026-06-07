using System.Text.Json;
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
        var buscaPorNumero = VendaNumeroHelper.TryParseNumeroBusca(filtro.Busca, out var numeroVenda);

        if (buscaPorNumero)
            return await ListarPorNumeroVendaAsync(filtro, numeroVenda, tamanho, cancelamento);

        if (!string.IsNullOrWhiteSpace(filtro.Busca) &&
            Guid.TryParse(filtro.Busca.Trim(), out var vendaId))
        {
            var unica = await ObterPorIdAsync(vendaId, cancelamento);
            if (unica.Sucesso && unica.Dados is not null &&
                PassaFiltrosLocais(unica.Dados, filtro))
            {
                return Resultado<ListaPaginada<VendaDto>>.Ok(new ListaPaginada<VendaDto>
                {
                    Itens = [unica.Dados],
                    Total = 1,
                    Pagina = 1,
                    ItensPorPagina = tamanho
                });
            }

            return Resultado<ListaPaginada<VendaDto>>.Ok(new ListaPaginada<VendaDto>
            {
                Itens = [],
                Total = 0,
                Pagina = Math.Max(1, filtro.Pagina),
                ItensPorPagina = tamanho
            });
        }

        var filtroSomentePeriodo = (filtro.Inicio.HasValue || filtro.Fim.HasValue) &&
                                   string.IsNullOrWhiteSpace(filtro.TipoItem);

        if (filtroSomentePeriodo && (filtro.ClienteId.HasValue || filtro.FuncionarioId.HasValue))
            return await ListarComFiltroIdentificadorAsync(filtro, tamanho, cancelamento);

        if (filtroSomentePeriodo)
            return await ListarComFiltroPeriodoAsync(filtro, tamanho, cancelamento);

        var temFiltroLocal = filtro.Inicio.HasValue || filtro.Fim.HasValue ||
                             !string.IsNullOrWhiteSpace(filtro.TipoItem);

        if (temFiltroLocal)
        {
            var acumulado = await BuscarPaginasParaFiltroLocalAsync(filtro, cancelamento);
            var filtrados = AplicarFiltrosLocais(acumulado, filtro);
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

    private async Task<Resultado<ListaPaginada<VendaDto>>> ListarComFiltroIdentificadorAsync(
        FiltroVenda filtro,
        int tamanho,
        CancellationToken cancelamento)
    {
        var acumulado = await BuscarPaginasIdentificadorAsync(filtro, cancelamento);
        var filtrados = AplicarFiltrosLocais(acumulado, filtro);
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

    private async Task<Resultado<ListaPaginada<VendaDto>>> ListarPorNumeroVendaAsync(
        FiltroVenda filtro,
        int numeroVenda,
        int tamanho,
        CancellationToken cancelamento)
    {
        var filtroApi = new FiltroVenda
        {
            ClienteId = filtro.ClienteId,
            FuncionarioId = filtro.FuncionarioId,
            TipoItem = filtro.TipoItem
        };

        var acumulado = await BuscarTodasVendasApiAsync(filtroApi, cancelamento);
        var filtrados = AplicarFiltrosLocais(acumulado, filtro, numeroVenda, ignorarPeriodo: true);
        var pagina = Math.Max(1, filtro.Pagina);
        var itens = filtrados.Skip((pagina - 1) * tamanho).Take(tamanho).ToList();
        foreach (var item in itens)
            item.Numero = numeroVenda;

        return Resultado<ListaPaginada<VendaDto>>.Ok(new ListaPaginada<VendaDto>
        {
            Itens = itens,
            Total = filtrados.Count,
            Pagina = pagina,
            ItensPorPagina = tamanho
        });
    }

    private async Task<List<VendaDto>> BuscarPaginasIdentificadorAsync(FiltroVenda filtro, CancellationToken cancelamento)
    {
        const int tamanhoPagina = 100;
        var todos = new List<VendaDto>();
        var pagina = 1;

        while (true)
        {
            var dados = await ObterPaginaVendasAsync(pagina, tamanhoPagina, filtro, cancelamento);
            if (dados is null || dados.Itens.Count == 0)
                break;

            todos.AddRange(dados.Itens);
            if (todos.Count >= dados.Total)
                break;

            pagina++;
        }

        return todos;
    }

    private async Task<List<VendaDto>> BuscarTodasVendasApiAsync(FiltroVenda filtro, CancellationToken cancelamento)
    {
        if (filtro.ClienteId.HasValue || filtro.FuncionarioId.HasValue)
            return await BuscarPaginasIdentificadorAsync(filtro, cancelamento);

        return await BuscarTodasPaginasVendasAsync(filtro, cancelamento);
    }

    private async Task<List<VendaDto>> BuscarTodasPaginasVendasAsync(FiltroVenda filtro, CancellationToken cancelamento)
    {
        const int tamanhoPagina = 100;
        var todos = new List<VendaDto>();
        var pagina = 1;

        while (true)
        {
            var dados = await ObterPaginaVendasAsync(pagina, tamanhoPagina, filtro, cancelamento);
            if (dados is null || dados.Itens.Count == 0)
                break;

            todos.AddRange(dados.Itens);
            if (todos.Count >= dados.Total)
                break;

            pagina++;
        }

        return todos;
    }

    private async Task<Resultado<ListaPaginada<VendaDto>>> ListarComFiltroPeriodoAsync(
        FiltroVenda filtro,
        int tamanho,
        CancellationToken cancelamento)
    {
        var acumulado = await BuscarPaginasComParadaPorDataAsync(filtro, cancelamento);
        var filtrados = AplicarFiltrosLocais(acumulado, filtro);
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

    private async Task<PaginacaoApi<VendaDto>?> ObterPaginaVendasAsync(
        int pagina,
        int tamanho,
        FiltroVenda filtro,
        CancellationToken cancelamento)
    {
        var query = MontarQuery(new Dictionary<string, string?>
        {
            ["pagina"] = pagina.ToString(),
            ["tamanho"] = tamanho.ToString(),
            ["funcionarioId"] = filtro.FuncionarioId?.ToString(),
            ["clienteId"] = filtro.ClienteId?.ToString()
        });

        var api = await GetAsync<PaginacaoApi<VendaDto>>($"vendas{query}", cancelamento);
        return api.Sucesso && api.Dados is not null ? api.Dados : null;
    }

    private async Task<List<VendaDto>> BuscarPaginasParalelasAsync(FiltroVenda filtro, CancellationToken cancelamento)
    {
        const int tamanhoPagina = 100;
        var primeira = await ObterPaginaVendasAsync(1, tamanhoPagina, filtro, cancelamento);
        if (primeira is null || primeira.Itens.Count == 0)
            return [];

        var todos = new List<VendaDto>(primeira.Itens);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(primeira.Total / (double)tamanhoPagina));

        if (totalPaginas > 1)
        {
            var tarefas = Enumerable.Range(2, totalPaginas - 1)
                .Select(async pagina =>
                {
                    var dados = await ObterPaginaVendasAsync(pagina, tamanhoPagina, filtro, cancelamento);
                    return dados?.Itens ?? [];
                });

            var paginas = await Task.WhenAll(tarefas);
            foreach (var itens in paginas)
                todos.AddRange(itens);
        }

        return todos;
    }

    private async Task<List<VendaDto>> BuscarPaginasComParadaPorDataAsync(FiltroVenda filtro, CancellationToken cancelamento)
    {
        const int tamanhoPagina = 100;
        var todos = new List<VendaDto>();
        var pagina = 1;
        var limiteInferior = filtro.Inicio?.Date;

        while (true)
        {
            var dados = await ObterPaginaVendasAsync(pagina, tamanhoPagina, filtro, cancelamento);
            if (dados is null || dados.Itens.Count == 0)
                break;

            var itens = dados.Itens;
            todos.AddRange(itens);

            if (todos.Count >= dados.Total || itens.Count < tamanhoPagina)
                break;

            if (limiteInferior.HasValue)
            {
                var dataMaisRecente = itens.Max(v => v.DataHora.ToLocalTime().Date);
                if (dataMaisRecente < limiteInferior.Value)
                    break;
            }

            pagina++;
        }

        return todos;
    }

    private async Task<List<VendaDto>> BuscarPaginasParaFiltroLocalAsync(FiltroVenda filtro, CancellationToken cancelamento)
    {
        if (filtro.ClienteId.HasValue || filtro.FuncionarioId.HasValue)
            return await BuscarPaginasParalelasAsync(filtro, cancelamento);

        return await BuscarPaginasComParadaPorDataAsync(filtro, cancelamento);
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

    private static bool PassaFiltrosLocais(VendaDto venda, FiltroVenda filtro)
    {
        if (filtro.FuncionarioId.HasValue && venda.FuncionarioId != filtro.FuncionarioId.Value)
            return false;
        if (filtro.ClienteId.HasValue && venda.ClienteId != filtro.ClienteId.Value)
            return false;
        if ((filtro.Inicio.HasValue || filtro.Fim.HasValue) &&
            !VendaCalculoHelper.VendaNoPeriodo(venda, filtro.Inicio, filtro.Fim))
            return false;
        if (!string.IsNullOrWhiteSpace(filtro.TipoItem) &&
            !VendaCalculoHelper.VendaContemTipo(venda, filtro.TipoItem))
            return false;
        return true;
    }

    private static List<VendaDto> AplicarFiltrosLocais(
        IEnumerable<VendaDto> itens,
        FiltroVenda filtro,
        int? numeroVenda = null,
        bool ignorarPeriodo = false)
    {
        var lista = itens.ToList();
        var query = lista.AsEnumerable();

        if (filtro.FuncionarioId.HasValue)
            query = query.Where(v => v.FuncionarioId == filtro.FuncionarioId.Value);

        if (filtro.ClienteId.HasValue)
            query = query.Where(v => v.ClienteId == filtro.ClienteId.Value);

        if (!ignorarPeriodo && (filtro.Inicio.HasValue || filtro.Fim.HasValue))
            query = query.Where(v => VendaCalculoHelper.VendaNoPeriodo(v, filtro.Inicio, filtro.Fim));

        if (!string.IsNullOrWhiteSpace(filtro.TipoItem))
            query = query.Where(v => VendaCalculoHelper.VendaContemTipo(v, filtro.TipoItem));

        if (numeroVenda.HasValue)
        {
            var ids = VendaNumeroHelper.BuscarIdsPorNumero(lista, numeroVenda.Value);
            query = query.Where(v => ids.Contains(v.Id));
        }

        return query.OrderByDescending(v => v.DataHora).ToList();
    }
}
