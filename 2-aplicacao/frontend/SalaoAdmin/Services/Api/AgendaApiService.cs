using SalaoAdmin.Comum;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Agenda;

namespace SalaoAdmin.Servicos.Api;

public class AgendaApiService(
    HttpClient httpClient,
    ErrorHandlerService errorHandler,
    ILogger<AgendaApiService> logger) : BaseApiService(httpClient, errorHandler, logger), IAgendaServico
{
    public async Task<Resultado<List<JanelaAgendaDto>>> ListarPorFuncionarioAsync(Guid funcionarioId, CancellationToken cancelamento = default)
    {
        var api = await GetAsync<List<JanelaAgendaDto>>($"agenda/funcionarios/{funcionarioId}", cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado<List<JanelaAgendaDto>>> DefinirAgendaAsync(Guid funcionarioId, DefinirAgendaDto dto, CancellationToken cancelamento = default)
    {
        if (TemConflitoInterno(dto.Janelas))
            return Resultado<List<JanelaAgendaDto>>.Falha("Existem janelas sobrepostas para o mesmo dia.");

        var payload = new DefinirAgendaEnvioDto
        {
            Janelas = dto.Janelas.Select(j => new JanelaAgendaEnvioDto
            {
                DiaSemana = j.DiaSemana,
                HoraInicio = j.HoraInicio,
                HoraFim = j.HoraFim
            }).ToList()
        };

        var api = await PutAsync<DefinirAgendaEnvioDto, List<JanelaAgendaDto>>($"agenda/funcionarios/{funcionarioId}", payload, cancelamento);
        return api.ParaResultado();
    }

    public async Task<Resultado> ExcluirJanelaAsync(Guid janelaId, CancellationToken cancelamento = default)
    {
        var api = await DeleteAsync<object>($"agenda/janelas/{janelaId}", cancelamento);
        return api.ParaResultadoBase();
    }

    private static bool TemConflitoInterno(IEnumerable<JanelaAgendaDto> janelas)
    {
        var porDia = janelas
            .GroupBy(x => x.DiaSemana)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var dia in porDia.Values)
        {
            var ordenado = dia
                .OrderBy(x => TimeOnly.Parse(x.HoraInicio))
                .ToList();

            for (var i = 1; i < ordenado.Count; i++)
            {
                var anteriorFim = TimeOnly.Parse(ordenado[i - 1].HoraFim);
                var atualInicio = TimeOnly.Parse(ordenado[i].HoraInicio);
                if (atualInicio < anteriorFim)
                    return true;
            }
        }

        return false;
    }
}
