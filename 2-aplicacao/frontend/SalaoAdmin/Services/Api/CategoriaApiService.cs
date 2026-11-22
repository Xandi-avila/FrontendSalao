using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Categorias;

namespace SalaoAdmin.Servicos.Api;

public class CategoriaApiService(
    HttpClient httpClient,
    ErrorHandlerService errorHandler,
    ILogger<CategoriaApiService> logger) : BaseApiService(httpClient, errorHandler, logger), ICategoriaServico
{
    public async Task<Resultado<ListaPaginada<CategoriaDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var query = MontarQuery(new Dictionary<string, string?>
        {
            ["pagina"] = filtro.Pagina.ToString(),
            ["tamanho"] = filtro.ItensPorPagina.ToString()
        });

        var api = await GetAsync<PaginacaoApi<CategoriaDto>>($"categorias{query}", cancelamento);
        if (!api.Sucesso || api.Dados is null)
            return Resultado<ListaPaginada<CategoriaDto>>.Falha(api.Erros.Count > 0 ? api.Erros : [api.Mensagem ?? "Falha ao listar categorias."]);

        var itens = api.Dados.Itens;
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim().ToLowerInvariant();
            itens = itens.Where(x => x.Nome.ToLowerInvariant().Contains(busca)).ToList();
        }

        return Resultado<ListaPaginada<CategoriaDto>>.Ok(new ListaPaginada<CategoriaDto>
        {
            Itens = itens,
            Total = string.IsNullOrWhiteSpace(filtro.Busca) ? api.Dados.Total : itens.Count,
            Pagina = api.Dados.Pagina,
            ItensPorPagina = api.Dados.Tamanho
        });
    }

    public async Task<Resultado<CategoriaDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await GetAsync<CategoriaDto>($"categorias/{id}", cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<CategoriaDto>> CriarAsync(CategoriaCadastroDto dto, CancellationToken cancelamento = default)
    {
        var api = await PostAsync<CategoriaCadastroDto, CategoriaDto>("categorias", dto, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<CategoriaDto>> AtualizarAsync(CategoriaEdicaoDto dto, CancellationToken cancelamento = default)
    {
        var payload = new CategoriaCadastroDto
        {
            Nome = dto.Nome,
            ServicoIds = dto.ServicoIds
        };
        var api = await PutAsync<CategoriaCadastroDto, CategoriaDto>($"categorias/{dto.Id}", payload, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await DeleteAsync<object>($"categorias/{id}", cancelamento);
        return api.ParaResultadoBase();
    }
}
