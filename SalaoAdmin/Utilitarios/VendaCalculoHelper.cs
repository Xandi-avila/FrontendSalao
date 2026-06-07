using SalaoAdmin.Dtos.Vendas;

namespace SalaoAdmin.Utilitarios;

public static class VendaCalculoHelper
{
    public static decimal CalcularSubtotal(IEnumerable<ItemCarrinhoVenda> itens) =>
        itens.Sum(i => i.Subtotal);

    public static decimal CalcularTotal(IEnumerable<ItemCarrinhoVenda> itens) =>
        CalcularSubtotal(itens);

    public static int ContarItensPorTipo(IEnumerable<VendaDto> vendas, string tipo) =>
        vendas.SelectMany(v => v.Itens).Where(i => string.Equals(i.Tipo, tipo, StringComparison.OrdinalIgnoreCase)).Sum(i => i.Quantidade);

    public static bool VendaContemTipo(VendaDto venda, string tipo) =>
        venda.Itens.Any(i => string.Equals(i.Tipo, tipo, StringComparison.OrdinalIgnoreCase));

    public static bool VendaNoPeriodo(VendaDto venda, DateTime? inicio, DateTime? fim)
    {
        if (inicio.HasValue && venda.DataHora.Date < inicio.Value.Date) return false;
        if (fim.HasValue && venda.DataHora.Date > fim.Value.Date) return false;
        return true;
    }

    public static VendaCadastroDto MontarPayload(Guid funcionarioId, Guid? clienteId, IEnumerable<ItemCarrinhoVenda> carrinho)
    {
        var inicio = DateTime.Now;
        if (inicio.Kind == DateTimeKind.Unspecified)
            inicio = DateTime.SpecifyKind(inicio, DateTimeKind.Local);

        return new VendaCadastroDto
        {
            FuncionarioId = funcionarioId,
            ClienteId = clienteId,
            DataHora = inicio.ToUniversalTime(),
            Itens = carrinho.Select(i => new ItemVendaCadastroDto
            {
                Tipo = i.Tipo,
                ProdutoId = i.Tipo == TipoItemVenda.Produto ? i.ReferenciaId : null,
                ServicoId = i.Tipo == TipoItemVenda.Servico ? i.ReferenciaId : null,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario
            }).ToList()
        };
    }
}

public sealed class ItemCarrinhoVenda
{
    public string Chave { get; init; } = Guid.NewGuid().ToString("N");
    public Guid ReferenciaId { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string Categoria { get; init; } = string.Empty;
    public decimal ValorUnitario { get; set; }
    public int Quantidade { get; set; } = 1;
    public decimal Subtotal => ValorUnitario * Quantidade;
}
