using SalaoAdmin.Dtos.Vendas;

namespace SalaoAdmin.Utilitarios;

public static class VendaCalculoHelper
{
    public static decimal CalcularSubtotal(IEnumerable<ItemCarrinhoVenda> itens) =>
        itens.Sum(i => i.Subtotal);

    public static decimal CalcularTotal(IEnumerable<ItemCarrinhoVenda> itens) =>
        CalcularSubtotal(itens);

    public static int ContarItensPorTipo(IEnumerable<VendaDto> vendas, string tipo) =>
        vendas.SelectMany(v => v.Itens).Where(i => ItemEhTipo(i, tipo)).Sum(i => Math.Max(1, i.Quantidade));

    public static (int Produtos, int Servicos) ResumoQuantidades(VendaDto venda)
    {
        var produtos = venda.Itens.Where(i => ItemEhTipo(i, TipoItemVenda.Produto)).Sum(i => Math.Max(1, i.Quantidade));
        var servicos = venda.Itens.Where(i => ItemEhTipo(i, TipoItemVenda.Servico)).Sum(i => Math.Max(1, i.Quantidade));
        return (produtos, servicos);
    }

    public static string TextoResumoItens(VendaDto venda)
    {
        var (prod, serv) = ResumoQuantidades(venda);
        if (prod > 0 && serv > 0) return $"{prod} prod. · {serv} serv.";
        if (prod > 0) return $"{prod} produto(s)";
        if (serv > 0) return $"{serv} serviço(s)";
        return "—";
    }

    private static bool ItemEhTipo(ItemVendaDto item, string tipo)
    {
        if (!string.IsNullOrWhiteSpace(item.Tipo) &&
            string.Equals(item.Tipo, tipo, StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(tipo, TipoItemVenda.Produto, StringComparison.OrdinalIgnoreCase))
            return item.ProdutoId.HasValue;

        if (string.Equals(tipo, TipoItemVenda.Servico, StringComparison.OrdinalIgnoreCase))
            return item.ServicoId.HasValue;

        return false;
    }

    public static bool VendaContemTipo(VendaDto venda, string tipo) =>
        venda.Itens.Any(i => ItemEhTipo(i, tipo));

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
