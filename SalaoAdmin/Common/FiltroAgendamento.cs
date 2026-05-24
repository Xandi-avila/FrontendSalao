namespace SalaoAdmin.Comum;

/// <summary>
/// Filtros alinhados ao GET /agendamentos (pagina, tamanho, funcionarioId, clienteId, inicio, fim).
/// </summary>
public class FiltroAgendamento : FiltroPaginacao
{
    public Guid? FuncionarioId { get; set; }
    public Guid? ClienteId { get; set; }
    public DateTime? Inicio { get; set; }
    public DateTime? Fim { get; set; }
}
