using SalaoAdmin.Components.Agenda;
using SalaoAdmin.Contratos;

namespace SalaoAdmin.Utilitarios;

public static class AnaliseCalculoHelper
{
    public static AgendaPainelGerencial.IndicadoresGerenciais CalcularLocal(
        DateTime inicio,
        DateTime fim,
        IReadOnlyList<AgendamentoResumoAnalise> agendamentos,
        Func<Guid, string> nomeFuncionario,
        Func<Guid, string> nomeServico)
    {
        var noPeriodo = agendamentos
            .Where(a => a.DataHoraInicio.Date >= inicio.Date && a.DataHoraInicio.Date <= fim.Date)
            .ToList();

        var ativos = noPeriodo.Where(a => !string.Equals(a.Status, "CANCELADO", StringComparison.OrdinalIgnoreCase)).ToList();
        var cancelados = noPeriodo.Where(a => string.Equals(a.Status, "CANCELADO", StringComparison.OrdinalIgnoreCase)).ToList();

        var porFunc = ativos.GroupBy(a => a.FuncionarioId)
            .Select(g => new { Id = g.Key, Qtd = g.Count() })
            .OrderByDescending(x => x.Qtd)
            .ToList();
        var maxFunc = porFunc.Count > 0 ? porFunc[0].Qtd : 0;

        var porServ = ativos.GroupBy(a => a.ServicoId)
            .Select(g => new { Id = g.Key, Qtd = g.Count() })
            .OrderByDescending(x => x.Qtd)
            .Take(8)
            .ToList();
        var maxServ = porServ.Count > 0 ? porServ[0].Qtd : 0;

        var porHora = ativos.GroupBy(a => a.DataHoraInicio.ToString("HH:00"))
            .Select(g => new { Hora = g.Key, Qtd = g.Count() })
            .OrderByDescending(x => x.Qtd)
            .Take(8)
            .ToList();
        var maxHora = porHora.Count > 0 ? porHora[0].Qtd : 0;

        var cancelPorFunc = cancelados.GroupBy(a => a.FuncionarioId)
            .Select(g => new { Id = g.Key, Qtd = g.Count() })
            .OrderByDescending(x => x.Qtd)
            .ToList();
        var maxCancel = cancelPorFunc.Count > 0 ? cancelPorFunc[0].Qtd : 0;

        var minutosOcupados = ativos.Count * 60;
        var minutosDisponiveis = Math.Max(1, porFunc.Count * 14 * 60);

        return new AgendaPainelGerencial.IndicadoresGerenciais
        {
            TotalAgendamentos = noPeriodo.Count,
            Confirmados = ativos.Count(a => string.Equals(a.Status, "AGENDADO", StringComparison.OrdinalIgnoreCase)),
            Cancelados = cancelados.Count,
            TaxaOcupacao = Math.Min(100, minutosOcupados * 100.0 / minutosDisponiveis),
            PorFuncionario = porFunc.Select(x => new AgendaPainelGerencial.ItemBarra
            {
                Nome = nomeFuncionario(x.Id),
                Quantidade = x.Qtd,
                Percentual = maxFunc > 0 ? x.Qtd * 100.0 / maxFunc : 0
            }).ToList(),
            PorServico = porServ.Select(x => new AgendaPainelGerencial.ItemBarra
            {
                Nome = nomeServico(x.Id),
                Quantidade = x.Qtd,
                Percentual = maxServ > 0 ? x.Qtd * 100.0 / maxServ : 0
            }).ToList(),
            PorHorario = porHora.Select(x => new AgendaPainelGerencial.ItemBarra
            {
                Nome = x.Hora,
                Quantidade = x.Qtd,
                Percentual = maxHora > 0 ? x.Qtd * 100.0 / maxHora : 0
            }).ToList(),
            CancelamentosPorFuncionario = cancelPorFunc.Select(x => new AgendaPainelGerencial.ItemBarra
            {
                Nome = nomeFuncionario(x.Id),
                Quantidade = x.Qtd,
                Percentual = maxCancel > 0 ? x.Qtd * 100.0 / maxCancel : 0
            }).ToList()
        };
    }
}
