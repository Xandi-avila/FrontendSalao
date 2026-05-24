using SalaoAdmin.Comum;
using SalaoAdmin.Components.Agenda;

namespace SalaoAdmin.Contratos;

public interface IAnaliseServico
{
    Task<Resultado<AgendaPainelGerencial.IndicadoresGerenciais>> ObterIndicadoresAsync(
        DateTime inicio,
        DateTime fim,
        IReadOnlyList<AgendamentoResumoAnalise> agendamentosFallback,
        Func<Guid, string> nomeFuncionario,
        Func<Guid, string> nomeServico,
        CancellationToken cancelamento = default);
}

public sealed class AgendamentoResumoAnalise
{
    public Guid FuncionarioId { get; init; }
    public Guid ServicoId { get; init; }
    public DateTime DataHoraInicio { get; init; }
    public string Status { get; init; } = string.Empty;
}
