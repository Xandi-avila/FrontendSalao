namespace SalaoAdmin.Comum;

public class FiltroAgendamento : FiltroPaginacao
{
    public Guid? FuncionarioId { get; set; }
    public Guid? ClienteId { get; set; }
    public DateTime? Inicio { get; set; }
    public DateTime? Fim { get; set; }
}
