namespace SalaoAdmin.Utilitarios;

public sealed class AgendaBuscaAgendamentoRequest
{
    public SugestaoFuncionarioVaga Sugestao { get; init; } = null!;
    public Guid ClienteId { get; init; }
    public Guid ServicoId { get; init; }
}
