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
                    ProfissoesHelper.ContemBusca(x.Profissao, busca) ||
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
        dto.Facebook = string.IsNullOrWhiteSpace(dto.Facebook) ? null : dto.Facebook.Trim();
        dto.Profissao = ProfissoesHelper.Limpar(dto.Profissao);
        dto.DataNascimento = ApiDateTimeHelper.NormalizarDataNascimento(dto.DataNascimento);
        return dto;
    }
}
