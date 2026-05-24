namespace SalaoAdmin.Dtos.Agendamentos;

public class AgendamentoDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid FuncionarioId { get; set; }
    public Guid ServicoId { get; set; }
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AgendamentoCadastroDto
{
    public Guid ClienteId { get; set; }
    public Guid FuncionarioId { get; set; }
    public Guid ServicoId { get; set; }
    public DateTime DataHoraInicio { get; set; }
}

public class AgendamentoEdicaoDto : AgendamentoCadastroDto
{
    public Guid Id { get; set; }
}
