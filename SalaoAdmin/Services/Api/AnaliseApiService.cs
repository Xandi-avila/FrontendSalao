using System.Text.Json;
using SalaoAdmin.Comum;
using SalaoAdmin.Components.Agenda;
using SalaoAdmin.Contratos;
using SalaoAdmin.Dtos.Analise;
using SalaoAdmin.Utilitarios;

namespace SalaoAdmin.Servicos.Api;

public class AnaliseApiService(
    HttpClient httpClient,
    ErrorHandlerService errorHandler,
    ILogger<AnaliseApiService> logger) : BaseApiService(httpClient, errorHandler, logger), IAnaliseServico
{
    public async Task<Resultado<ResumoFaturamentoVenda>> ObterFaturamentoAsync(
        DateTime inicio,
        DateTime fim,
        CancellationToken cancelamento = default)
    {
        var query = MontarQuery(new Dictionary<string, string?>
        {
            ["inicio"] = ApiDateTimeHelper.FormatarDataHoraApi(inicio.Date),
            ["fim"] = ApiDateTimeHelper.FormatarDataHoraApi(fim.Date.AddDays(1).AddTicks(-1))
        });

        var api = await GetAsync<JsonElement>($"analise/faturamento{query}", cancelamento);
        if (!api.Sucesso)
            return Resultado<ResumoFaturamentoVenda>.Falha(api.Mensagem ?? "Falha ao obter faturamento.");

        return Resultado<ResumoFaturamentoVenda>.Ok(ExtrairResumoFaturamento(api.Dados));
    }

    public async Task<Resultado<AgendaPainelGerencial.IndicadoresGerenciais>> ObterIndicadoresAsync(
        DateTime inicio,
        DateTime fim,
        IReadOnlyList<AgendamentoResumoAnalise> agendamentosFallback,
        Func<Guid, string> nomeFuncionario,
        Func<Guid, string> nomeServico,
        CancellationToken cancelamento = default)
    {
        var query = MontarQuery(new Dictionary<string, string?>
        {
            ["inicio"] = ApiDateTimeHelper.FormatarDataHoraApi(inicio.Date),
            ["fim"] = ApiDateTimeHelper.FormatarDataHoraApi(fim.Date.AddDays(1).AddTicks(-1))
        });

        var queryServicos = MontarQuery(new Dictionary<string, string?>
        {
            ["inicio"] = ApiDateTimeHelper.FormatarDataHoraApi(inicio.Date),
            ["fim"] = ApiDateTimeHelper.FormatarDataHoraApi(fim.Date.AddDays(1).AddTicks(-1)),
            ["limite"] = "10"
        });

        var funcApiTask = GetAsync<JsonElement>($"analise/funcionarios{query}", cancelamento);
        var servApiTask = GetAsync<JsonElement>($"analise/servicos{queryServicos}", cancelamento);
        await Task.WhenAll(funcApiTask, servApiTask);

        var funcApi = await funcApiTask;
        var servApi = await servApiTask;

        if (!funcApi.Sucesso && !servApi.Sucesso)
            return Resultado<AgendaPainelGerencial.IndicadoresGerenciais>.Ok(
                AnaliseCalculoHelper.CalcularLocal(inicio, fim, agendamentosFallback, nomeFuncionario, nomeServico));

        var porFuncionario = ExtrairItensBarra(funcApi.Dados, nomeFuncionario, nomeServico, preferirFuncionario: true);
        var porServico = ExtrairItensBarra(servApi.Dados, nomeFuncionario, nomeServico, preferirFuncionario: false);

        var ativos = agendamentosFallback.Where(a => !string.Equals(a.Status, "CANCELADO", StringComparison.OrdinalIgnoreCase)).ToList();
        var cancelados = agendamentosFallback.Where(a => string.Equals(a.Status, "CANCELADO", StringComparison.OrdinalIgnoreCase)).ToList();

        var porHora = ativos
            .GroupBy(a => a.DataHoraInicio.ToString("HH:00"))
            .Select(g => new { Hora = g.Key, Qtd = g.Count() })
            .OrderByDescending(x => x.Qtd)
            .Take(8)
            .ToList();
        var maxHora = porHora.Count > 0 ? porHora[0].Qtd : 0;

        var cancelPorFunc = cancelados
            .GroupBy(a => a.FuncionarioId)
            .Select(g => new { Id = g.Key, Qtd = g.Count() })
            .OrderByDescending(x => x.Qtd)
            .ToList();
        var maxCancel = cancelPorFunc.Count > 0 ? cancelPorFunc[0].Qtd : 0;

        var minutosOcupados = ativos.Count * 60;
        var minutosDisponiveis = Math.Max(1, cancelPorFunc.Count * 14 * 60);

        return Resultado<AgendaPainelGerencial.IndicadoresGerenciais>.Ok(new AgendaPainelGerencial.IndicadoresGerenciais
        {
            TotalAgendamentos = agendamentosFallback.Count,
            Confirmados = ativos.Count(a => string.Equals(a.Status, "AGENDADO", StringComparison.OrdinalIgnoreCase)),
            Cancelados = cancelados.Count,
            TaxaOcupacao = Math.Min(100, minutosOcupados * 100.0 / minutosDisponiveis),
            PorFuncionario = porFuncionario,
            PorServico = porServico,
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
        });
    }

    private static List<AgendaPainelGerencial.ItemBarra> ExtrairItensBarra(
        JsonElement? dados,
        Func<Guid, string> nomeFuncionario,
        Func<Guid, string> nomeServico,
        bool preferirFuncionario)
    {
        if (dados is null || dados.Value.ValueKind != JsonValueKind.Array)
            return [];

        var itens = new List<(string Nome, int Qtd)>();
        foreach (var el in dados.Value.EnumerateArray())
        {
            var nome = LerNome(el, preferirFuncionario, nomeFuncionario, nomeServico);
            var qtd = LerQuantidade(el);
            if (qtd > 0)
                itens.Add((nome, qtd));
        }

        var ordenado = itens.OrderByDescending(x => x.Qtd).Take(8).ToList();
        var max = ordenado.Count > 0 ? ordenado[0].Qtd : 0;
        return ordenado.Select(x => new AgendaPainelGerencial.ItemBarra
        {
            Nome = x.Nome,
            Quantidade = x.Qtd,
            Percentual = max > 0 ? x.Qtd * 100.0 / max : 0
        }).ToList();
    }

    private static string LerNome(
        JsonElement el,
        bool preferirFuncionario,
        Func<Guid, string> nomeFuncionario,
        Func<Guid, string> nomeServico)
    {
        if (el.TryGetProperty("nome", out var n) && n.ValueKind == JsonValueKind.String)
            return n.GetString() ?? "—";
        if (el.TryGetProperty("nomeFuncionario", out var nf) && nf.ValueKind == JsonValueKind.String)
            return nf.GetString() ?? "—";
        if (el.TryGetProperty("nomeServico", out var ns) && ns.ValueKind == JsonValueKind.String)
            return ns.GetString() ?? "—";

        if (preferirFuncionario && el.TryGetProperty("funcionarioId", out var fid) && Guid.TryParse(fid.GetString(), out var gF))
            return nomeFuncionario(gF);
        if (!preferirFuncionario && el.TryGetProperty("servicoId", out var sid) && Guid.TryParse(sid.GetString(), out var gS))
            return nomeServico(gS);

        return "—";
    }

    private static int LerQuantidade(JsonElement el)
    {
        if (el.TryGetProperty("quantidade", out var q) && q.TryGetInt32(out var qi))
            return qi;
        if (el.TryGetProperty("total", out var t) && t.TryGetInt32(out var ti))
            return ti;
        if (el.TryGetProperty("qtd", out var q2) && q2.TryGetInt32(out var q2i))
            return q2i;
        return 0;
    }

    private static ResumoFaturamentoVenda ExtrairResumoFaturamento(JsonElement? el)
    {
        if (el is null || el.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return new ResumoFaturamentoVenda();

        var dados = el.Value;
        var quantidade = 0;
        if (dados.TryGetProperty("quantidade", out var qtdEl) && qtdEl.TryGetInt32(out var qtd))
            quantidade = qtd;
        else if (dados.TryGetProperty("qtd", out var qtd2El) && qtd2El.TryGetInt32(out var qtd2))
            quantidade = qtd2;

        decimal valor = 0;
        if (dados.TryGetProperty("faturamento", out var fatEl) && fatEl.TryGetDecimal(out var fat))
            valor = fat;
        else if (dados.TryGetProperty("valor", out var valEl) && valEl.TryGetDecimal(out var val))
            valor = val;
        else if (dados.TryGetProperty("total", out var totalDecEl) && totalDecEl.ValueKind == JsonValueKind.Number && totalDecEl.TryGetDecimal(out var totalDec))
            valor = totalDec;

        return new ResumoFaturamentoVenda { Quantidade = quantidade, Valor = valor };
    }
}
