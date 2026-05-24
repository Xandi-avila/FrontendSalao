using SalaoAdmin.Comum;
using SalaoAdmin.Dtos.Agenda;

namespace SalaoAdmin.Contratos;

public interface IAgendaServico
{
    Task<Resultado<List<JanelaAgendaDto>>> ListarPorFuncionarioAsync(Guid funcionarioId, CancellationToken cancelamento = default);
    Task<Resultado<List<JanelaAgendaDto>>> DefinirAgendaAsync(Guid funcionarioId, DefinirAgendaDto dto, CancellationToken cancelamento = default);
    Task<Resultado> ExcluirJanelaAsync(Guid janelaId, CancellationToken cancelamento = default);
}
