namespace SalaoAdmin.Dtos.Agenda;

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
