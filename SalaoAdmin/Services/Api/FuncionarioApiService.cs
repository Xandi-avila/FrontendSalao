using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Funcionarios;

namespace SalaoAdmin.Servicos.Api;

public class FuncionarioApiService(
    HttpClient httpClient,
    ErrorHandlerService errorHandler,
    ILogger<FuncionarioApiService> logger) : BaseApiService(httpClient, errorHandler, logger), IFuncionarioServico
{
    public async Task<Resultado<ListaPaginada<FuncionarioDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var query = MontarQuery(new Dictionary<string, string?>
        {
            ["pagina"] = filtro.Pagina.ToString(),
            ["tamanho"] = filtro.ItensPorPagina.ToString()
        });

        var api = await GetAsync<PaginacaoApi<FuncionarioDto>>($"funcionarios{query}", cancelamento);
        if (!api.Sucesso || api.Dados is null)
            return Resultado<ListaPaginada<FuncionarioDto>>.Falha(api.Erros.Count > 0 ? api.Erros : [api.Mensagem ?? "Falha ao listar funcionários."]);

        var itens = api.Dados.Itens;
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim().ToLowerInvariant();
            itens = itens.Where(x =>
                    x.NomeCompleto.ToLowerInvariant().Contains(busca) ||
                    x.Email.ToLowerInvariant().Contains(busca) ||
                    x.ProfissaoCargo.ToLowerInvariant().Contains(busca))
                .ToList();
        }

        return Resultado<ListaPaginada<FuncionarioDto>>.Ok(new ListaPaginada<FuncionarioDto>
        {
            Itens = itens,
            Total = string.IsNullOrWhiteSpace(filtro.Busca) ? api.Dados.Total : itens.Count,
            Pagina = api.Dados.Pagina,
            ItensPorPagina = api.Dados.Tamanho
        });
    }

    public async Task<Resultado<FuncionarioDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await GetAsync<FuncionarioDto>($"funcionarios/{id}", cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<FuncionarioDto>> CriarAsync(FuncionarioCadastroDto dto, CancellationToken cancelamento = default)
    {
        var api = await PostAsync<FuncionarioCadastroDto, FuncionarioDto>("funcionarios", dto, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<FuncionarioDto>> AtualizarAsync(FuncionarioEdicaoDto dto, CancellationToken cancelamento = default)
    {
        var payload = new FuncionarioCadastroDto
        {
            NomeCompleto = dto.NomeCompleto,
            Endereco = dto.Endereco,
            Telefone = dto.Telefone,
            ProfissaoCargo = dto.ProfissaoCargo,
            Email = dto.Email,
            Senha = dto.Senha,
            DataNascimento = dto.DataNascimento,
            NivelPermissao = dto.NivelPermissao,
            Status = dto.Status
        };

        var api = await PutAsync<FuncionarioCadastroDto, FuncionarioDto>($"funcionarios/{dto.Id}", payload, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await DeleteAsync<object>($"funcionarios/{id}", cancelamento);
        return api.ParaResultadoBase();
    }
}
