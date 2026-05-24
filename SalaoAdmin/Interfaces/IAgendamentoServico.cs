using SalaoAdmin.Comum;
using SalaoAdmin.Dtos.Agendamentos;

namespace SalaoAdmin.Contratos;

public interface IAgendamentoServico : IServicoCrud<AgendamentoDto, AgendamentoCadastroDto, AgendamentoEdicaoDto>
{
    Task<Resultado> CancelarAsync(Guid id, CancellationToken cancelamento = default);
}
