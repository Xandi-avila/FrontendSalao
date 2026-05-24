using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Servicos;

namespace SalaoAdmin.Servicos.Api;

public class ServicoApiService(
    HttpClient httpClient,
    ErrorHandlerService errorHandler,
    ILogger<ServicoApiService> logger) : BaseApiService(httpClient, errorHandler, logger), IServicoCatalogoServico
{
    public async Task<Resultado<ListaPaginada<ServicoDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var query = MontarQuery(new Dictionary<string, string?>
        {
            ["pagina"] = filtro.Pagina.ToString(),
            ["tamanho"] = filtro.ItensPorPagina.ToString()
        });

        var api = await GetAsync<PaginacaoApi<ServicoDto>>($"servicos{query}", cancelamento);
        if (!api.Sucesso || api.Dados is null)
            return Resultado<ListaPaginada<ServicoDto>>.Falha(api.Erros.Count > 0 ? api.Erros : [api.Mensagem ?? "Falha ao listar serviços."]);

        var itens = api.Dados.Itens;
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim().ToLowerInvariant();
            itens = itens.Where(x =>
                    x.Nome.ToLowerInvariant().Contains(busca) ||
                    x.NomeCategoria.ToLowerInvariant().Contains(busca))
                .ToList();
        }

        return Resultado<ListaPaginada<ServicoDto>>.Ok(new ListaPaginada<ServicoDto>
        {
            Itens = itens,
            Total = string.IsNullOrWhiteSpace(filtro.Busca) ? api.Dados.Total : itens.Count,
            Pagina = api.Dados.Pagina,
            ItensPorPagina = api.Dados.Tamanho
        });
    }

    public async Task<Resultado<ServicoDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await GetAsync<ServicoDto>($"servicos/{id}", cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<ServicoDto>> CriarAsync(ServicoCadastroDto dto, CancellationToken cancelamento = default)
    {
        var api = await PostAsync<ServicoCadastroDto, ServicoDto>("servicos", dto, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<ServicoDto>> AtualizarAsync(ServicoEdicaoDto dto, CancellationToken cancelamento = default)
    {
        var payload = new ServicoCadastroDto
        {
            Nome = dto.Nome,
            DuracaoMinutos = dto.DuracaoMinutos,
            PrecoMinimo = dto.PrecoMinimo,
            CategoriaId = dto.CategoriaId,
            Status = dto.Status
        };
        var api = await PutAsync<ServicoCadastroDto, ServicoDto>($"servicos/{dto.Id}", payload, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await DeleteAsync<object>($"servicos/{id}", cancelamento);
        return api.ParaResultadoBase();
    }
}
