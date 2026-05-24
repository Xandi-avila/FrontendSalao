namespace SalaoAdmin.Dtos.Agenda;

public class JanelaAgendaDto
{
    public Guid Id { get; set; }
    public Guid FuncionarioId { get; set; }
    public int DiaSemana { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFim { get; set; } = string.Empty;
}

public class DefinirAgendaDto
{
    public List<JanelaAgendaDto> Janelas { get; set; } = [];
}
