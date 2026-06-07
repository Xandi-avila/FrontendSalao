using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Clientes;
using SalaoAdmin.Utilitarios;

namespace SalaoAdmin.Servicos.Api;

public class ClienteApiService(
    HttpClient httpClient,
    ErrorHandlerService errorHandler,
    ILogger<ClienteApiService> logger) : BaseApiService(httpClient, errorHandler, logger), IClienteServico
{
    public async Task<Resultado<ListaPaginada<ClienteDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var tamanho = Math.Clamp(filtro.ItensPorPagina, 1, 100);

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var todos = await BuscarTodasPaginasAsync(cancelamento);
            var busca = filtro.Busca.Trim().ToLowerInvariant();
            var filtrados = todos.Where(x =>
                    x.NomeCompleto.ToLowerInvariant().Contains(busca) ||
                    x.WhatsApp.Contains(busca) ||
                    x.Email?.ToLowerInvariant().Contains(busca) == true ||
                    x.Instagram?.ToLowerInvariant().Contains(busca) == true ||
                    ProfissoesHelper.ContemBusca(x.Profissao, busca) ||
                    x.Endereco.ToLowerInvariant().Contains(busca))
                .OrderBy(x => x.NomeCompleto)
                .ToList();

            var pagina = Math.Max(1, filtro.Pagina);
            var itens = filtrados.Skip((pagina - 1) * tamanho).Take(tamanho).ToList();

            return Resultado<ListaPaginada<ClienteDto>>.Ok(new ListaPaginada<ClienteDto>
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
            ["tamanho"] = tamanho.ToString()
        });

        var api = await GetAsync<PaginacaoApi<ClienteDto>>($"clientes{query}", cancelamento);
        if (!api.Sucesso || api.Dados is null)
            return Resultado<ListaPaginada<ClienteDto>>.Falha(api.Erros.Count > 0 ? api.Erros : [api.Mensagem ?? "Falha ao listar clientes."]);

        return Resultado<ListaPaginada<ClienteDto>>.Ok(new ListaPaginada<ClienteDto>
        {
            Itens = api.Dados.Itens,
            Total = api.Dados.Total,
            Pagina = api.Dados.Pagina,
            ItensPorPagina = api.Dados.Tamanho
        });
    }

    private async Task<List<ClienteDto>> BuscarTodasPaginasAsync(CancellationToken cancelamento)
    {
        var todos = new List<ClienteDto>();
        var pagina = 1;

        while (true)
        {
            var query = MontarQuery(new Dictionary<string, string?>
            {
                ["pagina"] = pagina.ToString(),
                ["tamanho"] = "100"
            });

            var api = await GetAsync<PaginacaoApi<ClienteDto>>($"clientes{query}", cancelamento);
            if (!api.Sucesso || api.Dados is null || api.Dados.Itens.Count == 0)
                break;

            todos.AddRange(api.Dados.Itens);
            if (todos.Count >= api.Dados.Total || api.Dados.Itens.Count < 100)
                break;

            pagina++;
        }

        return todos;
    }

    public async Task<Resultado<ClienteDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await GetAsync<ClienteDto>($"clientes/{id}", cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<ClienteDto>> CriarAsync(ClienteCadastroDto dto, CancellationToken cancelamento = default)
    {
        var payload = NormalizarCadastro(dto);
        var api = await PostAsync<ClienteCadastroDto, ClienteDto>("clientes", payload, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<ClienteDto>> AtualizarAsync(ClienteEdicaoDto dto, CancellationToken cancelamento = default)
    {
        var payload = NormalizarCadastro(dto);
        var api = await PutAsync<ClienteCadastroDto, ClienteDto>($"clientes/{dto.Id}", payload, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await DeleteAsync<object>($"clientes/{id}", cancelamento);
        return api.ParaResultadoBase();
    }

    private static ClienteCadastroDto NormalizarCadastro(ClienteCadastroDto dto)
    {
        dto.NomeCompleto = dto.NomeCompleto.Trim();
        dto.Endereco = dto.Endereco.Trim();
        dto.WhatsApp = FormatacaoCampos.Telefone(dto.WhatsApp);
        dto.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        dto.Instagram = string.IsNullOrWhiteSpace(dto.Instagram) ? null : dto.Instagram.Trim();
        dto.Profissao = ProfissoesHelper.Limpar(dto.Profissao);
        dto.DataNascimento = ApiDateTimeHelper.NormalizarDataNascimento(dto.DataNascimento);
        return dto;
    }
}
