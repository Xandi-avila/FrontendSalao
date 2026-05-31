namespace SalaoAdmin.Comum;

/// <summary>
/// GET /vendas — pagina, tamanho, funcionarioId, clienteId (API).
/// Inicio, Fim, TipoItem, Busca — filtros apenas no front (API não expõe).
/// </summary>
public class FiltroVenda : FiltroPaginacao
{
    public Guid? FuncionarioId { get; set; }
    public Guid? ClienteId { get; set; }
    public DateTime? Inicio { get; set; }
    public DateTime? Fim { get; set; }
    /// <summary>PRODUTO, SERVICO ou null para todos.</summary>
    public string? TipoItem { get; set; }
}
