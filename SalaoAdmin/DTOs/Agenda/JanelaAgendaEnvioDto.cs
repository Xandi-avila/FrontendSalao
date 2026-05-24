namespace SalaoAdmin.Dtos.Agenda;

/// <summary>
/// Payload de janela para PUT /agenda/funcionarios/{id} (DefinirAgenda).
/// A API exige diaSemana, horaInicio e horaFim — id é gerenciado pelo servidor.
/// </summary>
public class JanelaAgendaEnvioDto
{
    public int DiaSemana { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFim { get; set; } = string.Empty;
}

public class DefinirAgendaEnvioDto
{
    public List<JanelaAgendaEnvioDto> Janelas { get; set; } = [];
}
