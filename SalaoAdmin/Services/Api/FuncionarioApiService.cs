using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Funcionarios;
using SalaoAdmin.Utilitarios;

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
                    x.Telefone.ToLowerInvariant().Contains(busca) ||
                    x.Celular?.ToLowerInvariant().Contains(busca) == true ||
                    x.CPF?.Contains(busca, StringComparison.OrdinalIgnoreCase) == true ||
                    ProfissoesHelper.ContemBusca(x.Profissoes, busca))
                .ToList();
        }

        foreach (var item in itens)
            NormalizarTelefoneResposta(item);

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
        var resultado = api.ParaResultado();
        if (resultado.Sucesso && resultado.Dados is not null)
            NormalizarTelefoneResposta(resultado.Dados);
        return resultado;
    }

    public async Task<Resultado<FuncionarioDto>> CriarAsync(FuncionarioCadastroDto dto, CancellationToken cancelamento = default)
    {
        var payload = NormalizarCadastro(dto, edicao: false);
        var api = await PostAsync<FuncionarioCadastroDto, FuncionarioDto>("funcionarios", payload, cancelamento);
        var resultado = api.ParaResultado();
        if (resultado.Sucesso && resultado.Dados is not null)
            NormalizarTelefoneResposta(resultado.Dados);
        return resultado;
    }

    public async Task<Resultado<FuncionarioDto>> AtualizarAsync(FuncionarioEdicaoDto dto, CancellationToken cancelamento = default)
    {
        var payload = NormalizarCadastro(dto, edicao: true);
        var api = await PutAsync<FuncionarioCadastroDto, FuncionarioDto>($"funcionarios/{dto.Id}", payload, cancelamento);
        var resultado = api.ParaResultado();
        if (resultado.Sucesso && resultado.Dados is not null)
            NormalizarTelefoneResposta(resultado.Dados);
        return resultado;
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await DeleteAsync<object>($"funcionarios/{id}", cancelamento);
        return api.ParaResultadoBase();
    }

    private static FuncionarioCadastroDto NormalizarCadastro(FuncionarioCadastroDto dto, bool edicao)
    {
        dto.NomeCompleto = dto.NomeCompleto.Trim();
        dto.Email = dto.Email.Trim();
        dto.Endereco = dto.Endereco.Trim();
        dto.Telefone = FormatacaoCampos.Telefone(dto.Telefone);
        dto.Celular = string.IsNullOrWhiteSpace(dto.Celular) ? null : FormatacaoCampos.Telefone(dto.Celular);
        if (string.IsNullOrWhiteSpace(dto.Celular) && !string.IsNullOrWhiteSpace(dto.Telefone))
            dto.Celular = dto.Telefone;
        dto.CPF = FormatacaoCampos.CpfSomenteDigitos(dto.CPF);
        dto.DataNascimento = ApiDateTimeHelper.NormalizarDataNascimento(dto.DataNascimento);
        dto.DataAdmissao = ApiDateTimeHelper.NormalizarDataNascimento(dto.DataAdmissao);
        dto.Profissoes = ProfissoesHelper.Limpar(dto.Profissoes);

        if (edicao && string.IsNullOrWhiteSpace(dto.Senha))
            dto.Senha = null;

        return dto;
    }

    private static void NormalizarTelefoneResposta(FuncionarioDto funcionario)
    {
        if (string.IsNullOrWhiteSpace(funcionario.Celular) && !string.IsNullOrWhiteSpace(funcionario.Telefone))
            funcionario.Celular = funcionario.Telefone;
    }
}
