using SalaoAdmin.Comum;
using SalaoAdmin.Components.Agenda;
using SalaoAdmin.Contratos;
using SalaoAdmin.DadosMock;
using SalaoAdmin.Dtos.Analise;
using SalaoAdmin.Utilitarios;

namespace SalaoAdmin.Servicos.Mock;

public class AnaliseServicoMock(ArmazenamentoLocal dados) : IAnaliseServico
{
    public Task<Resultado<ResumoFaturamentoVenda>> ObterFaturamentoAsync(
        DateTime inicio,
        DateTime fim,
        CancellationToken cancelamento = default)
    {
        var vendas = dados.Vendas
            .Where(v => VendaCalculoHelper.VendaNoPeriodo(v, inicio, fim))
            .ToList();

        return Task.FromResult(Resultado<ResumoFaturamentoVenda>.Ok(new ResumoFaturamentoVenda
        {
            Quantidade = vendas.Count,
            Valor = vendas.Sum(v => v.Total)
        }));
    }

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
