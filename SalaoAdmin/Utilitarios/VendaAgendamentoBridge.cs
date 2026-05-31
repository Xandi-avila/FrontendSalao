namespace SalaoAdmin.Utilitarios;

/// <summary>
/// Ponte preparatória para integração futura Agendamento → Venda.
/// A API ainda não expõe endpoint de conversão; usar manualmente no PDV por enquanto.
/// </summary>
public static class VendaAgendamentoBridge
{
    public sealed class OrigemAgendamento
    {
        public Guid AgendamentoId { get; init; }
        public Guid ClienteId { get; init; }
        public Guid FuncionarioId { get; init; }
        public Guid ServicoId { get; init; }
        public string NomeServico { get; init; } = string.Empty;
        public string NomeCategoria { get; init; } = string.Empty;
        public decimal ValorUnitario { get; init; }
    }

    /// <summary>
    /// Converte um atendimento concluído em item de carrinho (uso futuro automatizado).
    /// </summary>
    public static ItemCarrinhoVenda? ParaItemCarrinho(OrigemAgendamento origem) =>
        new()
        {
            ReferenciaId = origem.ServicoId,
            Tipo = Dtos.Vendas.TipoItemVenda.Servico,
            Nome = origem.NomeServico,
            Categoria = origem.NomeCategoria,
            ValorUnitario = origem.ValorUnitario,
            Quantidade = 1,
            Observacao = $"Origem: agendamento {origem.AgendamentoId:N}"
        };
}
