using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Produtos;

namespace SalaoAdmin.Servicos.Api;

public class ProdutoApiService(
    HttpClient httpClient,
    ErrorHandlerService errorHandler,
    ILogger<ProdutoApiService> logger) : BaseApiService(httpClient, errorHandler, logger), IProdutoServico
{
    public async Task<Resultado<ListaPaginada<ProdutoDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var query = MontarQuery(new Dictionary<string, string?>
        {
            ["pagina"] = filtro.Pagina.ToString(),
            ["tamanho"] = filtro.ItensPorPagina.ToString()
        });

        var api = await GetAsync<PaginacaoApi<ProdutoDto>>($"produtos{query}", cancelamento);
        if (!api.Sucesso || api.Dados is null)
            return Resultado<ListaPaginada<ProdutoDto>>.Falha(api.Erros.Count > 0 ? api.Erros : [api.Mensagem ?? "Falha ao listar produtos."]);

        var itens = api.Dados.Itens;
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim().ToLowerInvariant();
            itens = itens.Where(x => x.Nome.ToLowerInvariant().Contains(busca)).ToList();
        }

        return Resultado<ListaPaginada<ProdutoDto>>.Ok(new ListaPaginada<ProdutoDto>
        {
            Itens = itens,
            Total = string.IsNullOrWhiteSpace(filtro.Busca) ? api.Dados.Total : itens.Count,
            Pagina = api.Dados.Pagina,
            ItensPorPagina = api.Dados.Tamanho
        });
    }

    public async Task<Resultado<ProdutoDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await GetAsync<ProdutoDto>($"produtos/{id}", cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<ProdutoDto>> CriarAsync(ProdutoCadastroDto dto, CancellationToken cancelamento = default)
    {
        var api = await PostAsync<ProdutoCadastroDto, ProdutoDto>("produtos", dto, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<ProdutoDto>> AtualizarAsync(ProdutoEdicaoDto dto, CancellationToken cancelamento = default)
    {
        var payload = new ProdutoCadastroDto
        {
            Nome = dto.Nome,
            Valor = dto.Valor,
            CaminhoImagem = dto.CaminhoImagem,
            Status = dto.Status
        };
        var api = await PutAsync<ProdutoCadastroDto, ProdutoDto>($"produtos/{dto.Id}", payload, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await DeleteAsync<object>($"produtos/{id}", cancelamento);
        return api.ParaResultadoBase();
    }
}
