using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Clientes;

namespace SalaoAdmin.Servicos.Api;

public class ClienteApiService(
    HttpClient httpClient,
    ErrorHandlerService errorHandler,
    ILogger<ClienteApiService> logger) : BaseApiService(httpClient, errorHandler, logger), IClienteServico
{
    public async Task<Resultado<ListaPaginada<ClienteDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var query = MontarQuery(new Dictionary<string, string?>
        {
            ["pagina"] = filtro.Pagina.ToString(),
            ["tamanho"] = filtro.ItensPorPagina.ToString()
        });

        var api = await GetAsync<PaginacaoApi<ClienteDto>>($"clientes{query}", cancelamento);
        if (!api.Sucesso || api.Dados is null)
            return Resultado<ListaPaginada<ClienteDto>>.Falha(api.Erros.Count > 0 ? api.Erros : [api.Mensagem ?? "Falha ao listar clientes."]);

        var itens = api.Dados.Itens;
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim().ToLowerInvariant();
            itens = itens.Where(x =>
                    x.NomeCompleto.ToLowerInvariant().Contains(busca) ||
                    x.WhatsApp.Contains(busca) ||
                    x.Email?.ToLowerInvariant().Contains(busca) == true ||
                    x.Instagram?.ToLowerInvariant().Contains(busca) == true ||
                    x.Facebook?.ToLowerInvariant().Contains(busca) == true ||
                    x.Profissoes.Any(p => p.ToLowerInvariant().Contains(busca)) ||
                    x.Endereco.ToLowerInvariant().Contains(busca))
                .ToList();
        }

        return Resultado<ListaPaginada<ClienteDto>>.Ok(new ListaPaginada<ClienteDto>
        {
            Itens = itens,
            Total = string.IsNullOrWhiteSpace(filtro.Busca) ? api.Dados.Total : itens.Count,
            Pagina = api.Dados.Pagina,
            ItensPorPagina = api.Dados.Tamanho
        });
    }

    public async Task<Resultado<ClienteDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await GetAsync<ClienteDto>($"clientes/{id}", cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<ClienteDto>> CriarAsync(ClienteCadastroDto dto, CancellationToken cancelamento = default)
    {
        var api = await PostAsync<ClienteCadastroDto, ClienteDto>("clientes", dto, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<ClienteDto>> AtualizarAsync(ClienteEdicaoDto dto, CancellationToken cancelamento = default)
    {
        var payload = new ClienteCadastroDto
        {
            NomeCompleto = dto.NomeCompleto,
            WhatsApp = dto.WhatsApp,
            Email = dto.Email,
            Instagram = dto.Instagram,
            Facebook = dto.Facebook,
            Profissoes = dto.Profissoes,
            DataNascimento = dto.DataNascimento,
            Endereco = dto.Endereco
        };
        var api = await PutAsync<ClienteCadastroDto, ClienteDto>($"clientes/{dto.Id}", payload, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await DeleteAsync<object>($"clientes/{id}", cancelamento);
        return api.ParaResultadoBase();
    }
}
