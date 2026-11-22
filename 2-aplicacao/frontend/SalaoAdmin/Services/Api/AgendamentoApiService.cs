using System.Text.Json.Serialization;
using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Agendamentos;
using SalaoAdmin.Utilitarios;

namespace SalaoAdmin.Servicos.Api;

public class AgendamentoApiService(
    HttpClient httpClient,
    ErrorHandlerService errorHandler,
    ILogger<AgendamentoApiService> logger) : BaseApiService(httpClient, errorHandler, logger), IAgendamentoServico
{
    public async Task<Resultado<ListaPaginada<AgendamentoDto>>> ListarAsync(FiltroPaginacao filtro, CancellationToken cancelamento = default)
    {
        var agendamentoFiltro = filtro as FiltroAgendamento;
        var tamanho = Math.Clamp(filtro.ItensPorPagina, 1, 100);
        var query = MontarQuery(new Dictionary<string, string?>
        {
            ["pagina"] = filtro.Pagina.ToString(),
            ["tamanho"] = tamanho.ToString(),
            ["funcionarioId"] = agendamentoFiltro?.FuncionarioId?.ToString(),
            ["clienteId"] = agendamentoFiltro?.ClienteId?.ToString(),
            ["inicio"] = ApiDateTimeHelper.FormatarDataHoraApi(agendamentoFiltro?.Inicio),
            ["fim"] = ApiDateTimeHelper.FormatarDataHoraApi(agendamentoFiltro?.Fim)
        });

        var api = await GetAsync<PaginacaoApi<AgendamentoDto>>($"agendamentos{query}", cancelamento);
        if (!api.Sucesso || api.Dados is null)
            return Resultado<ListaPaginada<AgendamentoDto>>.Falha(api.Erros.Count > 0 ? api.Erros : [api.Mensagem ?? "Falha ao listar agendamentos."]);

        var itens = api.Dados.Itens;
        foreach (var item in itens)
            NormalizarResposta(item);

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim().ToLowerInvariant();
            itens = itens.Where(x =>
                    x.Status.ToLowerInvariant().Contains(busca) ||
                    x.ClienteId.ToString().Contains(busca) ||
                    x.FuncionarioId.ToString().Contains(busca))
                .ToList();
        }

        return Resultado<ListaPaginada<AgendamentoDto>>.Ok(new ListaPaginada<AgendamentoDto>
        {
            Itens = itens,
            Total = string.IsNullOrWhiteSpace(filtro.Busca) ? api.Dados.Total : itens.Count,
            Pagina = api.Dados.Pagina,
            ItensPorPagina = api.Dados.Tamanho
        });
    }

    public async Task<Resultado<AgendamentoDto>> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await GetAsync<AgendamentoDto>($"agendamentos/{id}", cancelamento);
        var resultado = api.ParaResultado();
        if (resultado.Sucesso && resultado.Dados is not null)
            NormalizarResposta(resultado.Dados);
        return resultado;
    }

    public async Task<Resultado<AgendamentoDto>> CriarAsync(AgendamentoCadastroDto dto, CancellationToken cancelamento = default)
    {
        var api = await PostAsync<AgendamentoPayloadApi, AgendamentoDto>("agendamentos", MontarPayload(dto), cancelamento);
        var resultado = api.ParaResultado();
        if (resultado.Sucesso && resultado.Dados is not null)
            NormalizarResposta(resultado.Dados);
        return resultado;
    }

    public async Task<Resultado<AgendamentoDto>> AtualizarAsync(AgendamentoEdicaoDto dto, CancellationToken cancelamento = default)
    {
        var payload = MontarPayload(new AgendamentoCadastroDto
        {
            ClienteId = dto.ClienteId,
            FuncionarioId = dto.FuncionarioId,
            ServicoId = dto.ServicoId,
            DataHoraInicio = dto.DataHoraInicio
        });
        var api = await PutAsync<AgendamentoPayloadApi, AgendamentoDto>($"agendamentos/{dto.Id}", payload, cancelamento);
        var resultado = api.ParaResultado();
        if (resultado.Sucesso && resultado.Dados is not null)
            NormalizarResposta(resultado.Dados);
        return resultado;
    }

    private static AgendamentoPayloadApi MontarPayload(AgendamentoCadastroDto dto)
    {
        var inicio = dto.DataHoraInicio;
        if (inicio.Kind == DateTimeKind.Unspecified)
            inicio = DateTime.SpecifyKind(inicio, DateTimeKind.Local);

        return new AgendamentoPayloadApi
        {
            ClienteId = dto.ClienteId,
            FuncionarioId = dto.FuncionarioId,
            ServicoIds = dto.ServicoId != Guid.Empty ? [dto.ServicoId] : [],
            DataHoraInicio = inicio.ToUniversalTime()
        };
    }

    private static void NormalizarResposta(AgendamentoDto dto)
    {
        if (dto.ServicoIds.Count > 0)
            dto.ServicoId = dto.ServicoIds[0];
        else if (dto.ServicoId != Guid.Empty)
            dto.ServicoIds = [dto.ServicoId];
    }

    private sealed class AgendamentoPayloadApi
    {
        public Guid ClienteId { get; set; }
        public Guid FuncionarioId { get; set; }

        [JsonPropertyName("servicoIds")]
        public List<Guid> ServicoIds { get; set; } = [];

        public DateTime DataHoraInicio { get; set; }
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await DeleteAsync<object>($"agendamentos/{id}", cancelamento);
        return api.ParaResultadoBase();
    }

    public async Task<Resultado> CancelarAsync(Guid id, CancellationToken cancelamento = default)
    {
        var api = await PatchAsync<object>($"agendamentos/{id}/cancelar", cancelamento);
        return api.ParaResultadoBase();
    }
}
