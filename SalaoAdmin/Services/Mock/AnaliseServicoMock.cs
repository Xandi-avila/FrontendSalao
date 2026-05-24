using SalaoAdmin.Comum;
using SalaoAdmin.Components.Agenda;
using SalaoAdmin.Contratos;
using SalaoAdmin.Utilitarios;

namespace SalaoAdmin.Servicos.Mock;

public class AnaliseServicoMock : IAnaliseServico
{
    public Task<Resultado<AgendaPainelGerencial.IndicadoresGerenciais>> ObterIndicadoresAsync(
        DateTime inicio,
        DateTime fim,
        IReadOnlyList<AgendamentoResumoAnalise> agendamentosFallback,
        Func<Guid, string> nomeFuncionario,
        Func<Guid, string> nomeServico,
        CancellationToken cancelamento = default)
    {
        var indicadores = AnaliseCalculoHelper.CalcularLocal(
            inicio, fim, agendamentosFallback, nomeFuncionario, nomeServico);
        return Task.FromResult(Resultado<AgendaPainelGerencial.IndicadoresGerenciais>.Ok(indicadores));
    }
}
