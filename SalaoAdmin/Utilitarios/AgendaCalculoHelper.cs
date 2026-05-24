using SalaoAdmin.Dtos.Agenda;
using SalaoAdmin.Dtos.Agendamentos;

namespace SalaoAdmin.Utilitarios;

public sealed class SlotHorarioLivre
{
    public Guid FuncionarioId { get; init; }
    public string NomeFuncionario { get; init; } = string.Empty;
    public DateTime Inicio { get; init; }
    public DateTime Fim { get; init; }
}

public sealed class SugestaoFuncionarioVaga
{
    public Guid FuncionarioId { get; init; }
    public string NomeFuncionario { get; init; } = string.Empty;
    public DateTime HorarioInicio { get; init; }
    public DateTime HorarioFim { get; init; }
    public int MinutosAteHorarioDesejado { get; init; }
}

public sealed class ConflitoAgendamentoInfo
{
    public Guid AgendamentoExistenteId { get; init; }
    public DateTime InicioExistente { get; init; }
    public DateTime FimExistente { get; init; }
    public bool SobreposicaoTotal { get; init; }
    public bool SobreposicaoParcial { get; init; }
    public string Mensagem { get; init; } = string.Empty;
}

public sealed class AnaliseAgendamentoResultado
{
    public List<ConflitoAgendamentoInfo> Conflitos { get; init; } = [];
    public List<string> Alertas { get; init; } = [];
    public bool ForaHorarioSugerido { get; init; }
    public bool TemConflito => Conflitos.Count > 0;
    public bool RequerConfirmacao => TemConflito || ForaHorarioSugerido || Alertas.Count > 0;
}

public static class AgendaCalculoHelper
{
    public const int PassoSlotMinutosPadrao = 15;
    public const int HoraCalendarioInicio = 7;
    public const int HoraCalendarioFim = 21;
    public const string HoraPadraoInicio = "07:00";
    public const string HoraPadraoFim = "21:00";

    public static bool IntervalosConflitam(DateTime inicioA, DateTime fimA, DateTime inicioB, DateTime fimB) =>
        inicioA < fimB && fimA > inicioB;

    public static List<SlotHorarioLivre> CalcularHorariosLivres(
        DateTime data,
        Guid funcionarioId,
        string nomeFuncionario,
        int duracaoMinutos,
        IReadOnlyList<JanelaAgendaDto> janelas,
        IReadOnlyList<AgendamentoDto> agendamentos,
        Func<AgendamentoDto, DateTime> obterFimAgendamento,
        int passoMinutos = PassoSlotMinutosPadrao)
    {
        if (duracaoMinutos <= 0)
            return [];

        var janelasEfetivas = ObterJanelasEfetivas(data, janelas);
        var agendamentosDia = agendamentos
            .Where(a => a.FuncionarioId == funcionarioId)
            .Where(a => !AgendamentoCancelado(a))
            .Where(a => a.DataHoraInicio.Date == data.Date)
            .ToList();

        var intervalosLivres = ObterIntervalosLivres(data, janelasEfetivas, agendamentosDia, obterFimAgendamento);
        var slots = new List<SlotHorarioLivre>();

        foreach (var (inicioLivre, fimLivre) in intervalosLivres)
        {
            var cursor = inicioLivre;
            while (cursor.AddMinutes(duracaoMinutos) <= fimLivre)
            {
                slots.Add(new SlotHorarioLivre
                {
                    FuncionarioId = funcionarioId,
                    NomeFuncionario = nomeFuncionario,
                    Inicio = cursor,
                    Fim = cursor.AddMinutes(duracaoMinutos)
                });
                cursor = cursor.AddMinutes(passoMinutos);
            }
        }

        return slots;
    }

    public static List<SugestaoFuncionarioVaga> BuscarFuncionariosComVaga(
        DateTime data,
        int duracaoMinutos,
        TimeSpan? horaDesejada,
        IEnumerable<(Guid Id, string Nome)> funcionarios,
        IReadOnlyDictionary<Guid, List<JanelaAgendaDto>> janelasPorFuncionario,
        IReadOnlyList<AgendamentoDto> agendamentos,
        Func<AgendamentoDto, DateTime> obterFimAgendamento,
        int passoMinutos = PassoSlotMinutosPadrao)
    {
        var sugestoes = new List<SugestaoFuncionarioVaga>();
        var horaAlvo = horaDesejada.HasValue ? data.Date.Add(horaDesejada.Value) : (DateTime?)null;

        foreach (var (id, nome) in funcionarios)
        {
            janelasPorFuncionario.TryGetValue(id, out var janelas);
            janelas ??= [];

            var slots = CalcularHorariosLivres(
                data, id, nome, duracaoMinutos, janelas, agendamentos, obterFimAgendamento, passoMinutos);

            foreach (var slot in slots)
            {
                var minutosDiff = horaAlvo.HasValue
                    ? (int)Math.Abs((slot.Inicio - horaAlvo.Value).TotalMinutes)
                    : 0;

                sugestoes.Add(new SugestaoFuncionarioVaga
                {
                    FuncionarioId = id,
                    NomeFuncionario = nome,
                    HorarioInicio = slot.Inicio,
                    HorarioFim = slot.Fim,
                    MinutosAteHorarioDesejado = minutosDiff
                });
            }
        }

        return sugestoes
            .OrderBy(s => s.MinutosAteHorarioDesejado)
            .ThenBy(s => s.HorarioInicio)
            .Take(40)
            .ToList();
    }

    public static AnaliseAgendamentoResultado AnalisarNovoAgendamento(
        Guid funcionarioId,
        DateTime inicio,
        int duracaoMinutos,
        IReadOnlyList<JanelaAgendaDto> janelas,
        IReadOnlyList<AgendamentoDto> agendamentos,
        Func<AgendamentoDto, DateTime> obterFimAgendamento,
        Guid? ignorarAgendamentoId = null)
    {
        var resultado = new AnaliseAgendamentoResultado();
        if (duracaoMinutos <= 0)
        {
            resultado.Alertas.Add("Serviço sem duração cadastrada. Informe a duração no cadastro de serviços.");
            return resultado;
        }

        var fim = inicio.AddMinutes(duracaoMinutos);
        var alertas = new List<string>();
        var conflitos = new List<ConflitoAgendamentoInfo>();

        if (!EstaDentroHorarioSugerido(inicio, fim, janelas))
        {
            alertas.Add("Horário fora da faixa sugerida (seg–sáb, 07:00–21:00 ou janelas cadastradas). Você pode continuar — flexibilizando a agenda.");
        }

        var agendamentosNoDia = agendamentos
            .Where(a => a.FuncionarioId == funcionarioId)
            .Where(a => !AgendamentoCancelado(a))
            .Where(a => a.DataHoraInicio.Date == inicio.Date)
            .Where(a => ignorarAgendamentoId is null || a.Id != ignorarAgendamentoId);

        foreach (var existente in agendamentosNoDia)
        {
            var fimExistente = obterFimAgendamento(existente);
            if (!IntervalosConflitam(inicio, fim, existente.DataHoraInicio, fimExistente))
                continue;

            var total = inicio <= existente.DataHoraInicio && fim >= fimExistente;
            var parcial = !total;

            conflitos.Add(new ConflitoAgendamentoInfo
            {
                AgendamentoExistenteId = existente.Id,
                InicioExistente = existente.DataHoraInicio,
                FimExistente = fimExistente,
                SobreposicaoTotal = total,
                SobreposicaoParcial = parcial,
                Mensagem = total
                    ? $"Sobreposição total com atendimento das {existente.DataHoraInicio:HH:mm} às {fimExistente:HH:mm}."
                    : $"Conflito parcial com atendimento das {existente.DataHoraInicio:HH:mm} às {fimExistente:HH:mm}."
            });
        }

        return new AnaliseAgendamentoResultado
        {
            Conflitos = conflitos,
            Alertas = alertas,
            ForaHorarioSugerido = alertas.Count > 0
        };
    }

    public static HashSet<Guid> DetectarAgendamentosComConflito(
        IReadOnlyList<AgendamentoDto> agendamentos,
        Func<AgendamentoDto, DateTime> obterFimAgendamento)
    {
        var ids = new HashSet<Guid>();
        var ativos = agendamentos.Where(a => !AgendamentoCancelado(a)).ToList();

        for (var i = 0; i < ativos.Count; i++)
        {
            for (var j = i + 1; j < ativos.Count; j++)
            {
                var a = ativos[i];
                var b = ativos[j];
                if (a.FuncionarioId != b.FuncionarioId || a.DataHoraInicio.Date != b.DataHoraInicio.Date)
                    continue;

                if (IntervalosConflitam(a.DataHoraInicio, obterFimAgendamento(a), b.DataHoraInicio, obterFimAgendamento(b)))
                {
                    ids.Add(a.Id);
                    ids.Add(b.Id);
                }
            }
        }

        return ids;
    }

    public static List<JanelaAgendaDto> ObterJanelasEfetivas(DateTime data, IReadOnlyList<JanelaAgendaDto> janelasCadastradas)
    {
        var diaSemana = NormalizarDiaSemana((int)data.DayOfWeek);
        var doDia = janelasCadastradas.Where(j => NormalizarDiaSemana(j.DiaSemana) == diaSemana).ToList();
        if (doDia.Count > 0)
            return doDia;

        if (diaSemana == 0)
            return [];

        return
        [
            new JanelaAgendaDto
            {
                DiaSemana = diaSemana,
                HoraInicio = HoraPadraoInicio,
                HoraFim = HoraPadraoFim
            }
        ];
    }

    public static bool EstaDentroHorarioSugerido(DateTime inicio, DateTime fim, IReadOnlyList<JanelaAgendaDto> janelasCadastradas)
    {
        var janelas = ObterJanelasEfetivas(inicio, janelasCadastradas);
        if (janelas.Count == 0)
            return true;

        var hi = TimeOnly.FromDateTime(inicio);
        var hf = TimeOnly.FromDateTime(fim);

        return janelas.Any(j =>
            TryParseHora(j.HoraInicio, out var ji) && TryParseHora(j.HoraFim, out var jf) && hi >= ji && hf <= jf);
    }

    public static List<(DateTime Inicio, DateTime Fim)> ObterIntervalosLivres(
        DateTime data,
        IReadOnlyList<JanelaAgendaDto> janelas,
        IReadOnlyList<AgendamentoDto> agendamentosDia,
        Func<AgendamentoDto, DateTime> obterFimAgendamento)
    {
        var diaSemana = NormalizarDiaSemana((int)data.DayOfWeek);
        var intervalos = new List<(DateTime Inicio, DateTime Fim)>();

        foreach (var janela in janelas.Where(j => NormalizarDiaSemana(j.DiaSemana) == diaSemana))
        {
            if (!TryParseHora(janela.HoraInicio, out var hi) || !TryParseHora(janela.HoraFim, out var hf))
                continue;

            intervalos.Add((data.Date.Add(hi.ToTimeSpan()), data.Date.Add(hf.ToTimeSpan())));
        }

        foreach (var ag in agendamentosDia.OrderBy(a => a.DataHoraInicio))
        {
            var agFim = obterFimAgendamento(ag);
            intervalos = SubtrairIntervalo(intervalos, ag.DataHoraInicio, agFim);
        }

        return intervalos;
    }

    private static List<(DateTime Inicio, DateTime Fim)> SubtrairIntervalo(
        List<(DateTime Inicio, DateTime Fim)> intervalos,
        DateTime ocupadoInicio,
        DateTime ocupadoFim)
    {
        var resultado = new List<(DateTime Inicio, DateTime Fim)>();

        foreach (var (ini, fim) in intervalos)
        {
            if (ocupadoFim <= ini || ocupadoInicio >= fim)
            {
                resultado.Add((ini, fim));
                continue;
            }

            if (ocupadoInicio > ini)
                resultado.Add((ini, ocupadoInicio));

            if (ocupadoFim < fim)
                resultado.Add((ocupadoFim, fim));
        }

        return resultado;
    }

    public static bool AgendamentoCancelado(AgendamentoDto agendamento) =>
        string.Equals(agendamento.Status, "CANCELADO", StringComparison.OrdinalIgnoreCase);

    public static int NormalizarDiaSemana(int diaSemana)
    {
        if (diaSemana is >= 0 and <= 6)
            return diaSemana;
        if (diaSemana == 7)
            return 0;
        if (diaSemana is >= 1 and <= 7)
            return diaSemana - 1;
        return 0;
    }

    public static bool TryParseHora(string? horaTexto, out TimeOnly hora)
    {
        hora = default;
        if (string.IsNullOrWhiteSpace(horaTexto))
            return false;

        if (TimeOnly.TryParse(horaTexto, out hora))
            return true;

        if (DateTime.TryParse(horaTexto, out var dataHora))
        {
            hora = TimeOnly.FromDateTime(dataHora);
            return true;
        }

        return false;
    }

    public static DateTime ObterInicioSemana(DateTime data)
    {
        var diff = ((int)data.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return data.Date.AddDays(-diff);
    }

    public static DateTime ObterInicioMes(DateTime data) => new(data.Year, data.Month, 1);

    public static IEnumerable<DateTime> DiasDaSemana(DateTime dataReferencia)
    {
        var inicio = ObterInicioSemana(dataReferencia);
        for (var i = 0; i < 7; i++)
            yield return inicio.AddDays(i);
    }

    public static IEnumerable<DateTime> DiasDoMes(DateTime dataReferencia)
    {
        var inicio = ObterInicioMes(dataReferencia);
        var fim = inicio.AddMonths(1);
        for (var d = inicio; d < fim; d = d.AddDays(1))
            yield return d;
    }

    public static IEnumerable<DateTime> DiasGradeMes(DateTime dataReferencia)
    {
        var inicioMes = ObterInicioMes(dataReferencia);
        var inicioGrade = ObterInicioSemana(inicioMes);
        for (var i = 0; i < 42; i++)
            yield return inicioGrade.AddDays(i);
    }
}
